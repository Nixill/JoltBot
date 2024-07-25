using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Nixill.Collections;
using Websocket.Client;

namespace Nixill.Streaming.JoltBot.OBS;

public static class OBSClient
{
  internal static WebsocketClient Client;
  static ILogger Logger = Log.Factory.CreateLogger(typeof(OBSClient));

  internal static string Password;

  internal static bool IsConnected = false;

  public static async Task SetUp()
  {
    JsonObject OBSConfig = (JsonObject)JsonNode.Parse(File.ReadAllText("data/obs.json"));

    RegisterEvents();

    Uri uri = new($"ws://{OBSConfig["server"]["ip"]}:{OBSConfig["server"]["port"]}/");
    Password = (string)OBSConfig["server"]["password"];

    Client = new WebsocketClient(uri);
    Client.ReconnectTimeout = TimeSpan.FromSeconds(15);
    Client.MessageReceived.Subscribe(Dispatch);
    Client.DisconnectionHappened.Subscribe(CheckDisconnect);
    Client.ReconnectionHappened.Subscribe(CheckReconnect);
    Client.IsReconnectionEnabled = false;

    await Client.Start();

    IsConnected = true;
  }

  private static void CheckDisconnect(DisconnectionInfo info)
  {
    Logger.LogError($"Disconnected. Status: {info.CloseStatus} // Comment: {info.CloseStatusDescription}");
    int code = (int)info.CloseStatus;
    if (code == 4011 || code == 4009)
    {
      Logger.LogWarning("Cannot reconnect on this close code.");
    }
    else
    {
      Logger.LogInformation("Attempting to reconnect.");
      Client.Reconnect();
    }
  }

  private static void CheckReconnect(ReconnectionInfo info)
  {
    Logger.LogInformation("Reconnected.");
    IsConnected = true;
  }

  static DictionaryGenerator<String, List<Action<JsonObject>>> EventHandlers;

  public static void RegisterEvents()
  {
    IEnumerable<MethodInfo> events = EventHandlerAttribute.GetMethods(typeof(OBSClient));

    EventHandlers = new EmptyConstructorGenerator<String, List<Action<JsonObject>>>().Wrap();

    foreach (MethodInfo m in events)
    {
      try
      {
        var pars = m.GetParameters();
        if (!m.IsStatic) throw new IllegalEventHandlerException(m, "It must be a static method.");
        if (pars.Length != 1) throw new IllegalEventHandlerException(m, "It must have exactly one parameter.");
        if (pars[0].ParameterType != typeof(JsonObject))
          throw new IllegalEventHandlerException(m, "Its sole parameter must be a JsonObject.");

        EventHandlerAttribute attr = m.GetCustomAttribute<EventHandlerAttribute>();
        EventHandlers[attr.EventName].Add(m.CreateDelegate<Action<JsonObject>>());
      }
      catch (IllegalEventHandlerException ex)
      {
        Logger.LogError(ex, "Could not register event handler");
      }
    }
  }

  public static void Dispatch(ResponseMessage msg)
  {
    JsonObject response = (JsonObject)JsonNode.Parse(msg.Text);
    OBSOpCode opcode = (OBSOpCode)(int)response["op"];
    JsonObject data = (JsonObject)response["d"];

    switch (opcode)
    {
      case OBSOpCode.Hello:
        Logger.LogInformation("Server said hello.");
        HandleHello(data); break;
      case OBSOpCode.Identified:
        Logger.LogInformation("Successfully identified!");
        break;
      case OBSOpCode.Event:
        HandleEvent(data); break;
      case OBSOpCode.RequestResponse:
        // case OBSOpCode.RequestBatchResponse:
        HandleResponse(data); break;
    }
  }

  static void HandleHello(JsonObject data)
  {
    JsonObject identify = new JsonObject
    {
      ["op"] = (int)OBSOpCode.Identify,
      ["d"] = new JsonObject
      {
        ["rpcVersion"] = 1
      }
    };

    // Get authentication info
    if (data.ContainsKey("authentication"))
    {
      SHA256 encoder = SHA256.Create();
      string password = OBSClient.Password;
      string salt = (string)data["authentication"]["salt"];
      string salted_pass = Convert.ToBase64String(encoder.ComputeHash(Encoding.UTF8.GetBytes(password + salt)));
      string challenge = (string)data["authentication"]["challenge"];
      string authKey = Convert.ToBase64String(encoder.ComputeHash(Encoding.UTF8.GetBytes(salted_pass + challenge)));

      identify["d"]["authentication"] = authKey;
    }

    OBSClient.Client.SendInstant(identify.ToString());

    Logger.LogInformation("Sent identify.");
  }

  static void HandleEvent(JsonObject data)
  {
    string eventName = (string)data["eventType"];
    JsonObject eventData = (JsonObject)data["eventData"];

    if (EventHandlers.ContainsKey(eventName))
    {
      Logger.LogDebug($"Received event {eventName}, dispatching.");
      foreach (var handler in EventHandlers[eventName])
      {
        Task _ = Task.Run(() => handler(data));
      }
    }
    else
    {
      Logger.LogTrace($"Received event {eventName}, unhandled.");
    }
  }

  static Dictionary<string, TaskCompletionSource<JsonObject>> WaitingResponses = new();

  public static Task<JsonObject> SendRequest(string requestType, JsonObject requestData)
  {
    string id = Guid.NewGuid().ToString();
    TaskCompletionSource<JsonObject> dataTask = new();
    JsonObject request = new JsonObject
    {
      ["op"] = (int)OBSOpCode.Request,
      ["d"] = new JsonObject
      {
        ["requestType"] = requestType,
        ["requestId"] = id,
        ["requestData"] = requestData
      }
    };

    WaitingResponses[id] = dataTask;
    Client.SendInstant(request.ToString());

    if (dataTask.Task.Wait(30_000))
    {
      return Task.FromResult(dataTask.Task.Result);
    }
    else
    {
      throw new OBSRequestTimedOutException(id);
    }
  }

  static void HandleResponse(JsonObject data)
  {
    string requestId = (string)data["requestId"];
    JsonObject requestStatus = (JsonObject)data["requestStatus"];
    bool isSuccessful = (bool)requestStatus["result"];

    if (WaitingResponses.ContainsKey(requestId))
    {
      var response = WaitingResponses[requestId];
      WaitingResponses.Remove(requestId);
      if (isSuccessful)
      {
        JsonObject responseData = (JsonObject)data["responseData"];
        response.SetResult(responseData);
      }
      else
      {
        int resultCode = (int)requestStatus["code"];
        string comment = (string)requestStatus["comment"];
        response.SetException(new OBSRequestFailedException(resultCode, comment));
      }
    }
    else
    {
      Logger.LogWarning($"Received response to request {requestId} of type {(data["requestType"]
        )}, which wasn't queued for a response.");
    }
  }
}

public enum OBSOpCode
{
  Hello = 0,
  Identify = 1,
  Identified = 2,
  Reidentify = 3,
  // skip 4
  Event = 5,
  Request = 6,
  RequestResponse = 7,
  RequestBatch = 8,
  RequestBatchResponse = 9
}

public static class OBSOpCodeExtensions
{
  public static bool IsIncoming(this OBSOpCode code) => code switch
  {
    OBSOpCode.Hello or OBSOpCode.Identified or OBSOpCode.Event
      or OBSOpCode.RequestResponse or OBSOpCode.RequestBatchResponse => true,
    _ => false
  };

  public static bool IsOutgoing(this OBSOpCode code) => code switch
  {
    OBSOpCode.Identify or OBSOpCode.Reidentify or OBSOpCode.Request or OBSOpCode.RequestBatch => true,
    _ => false
  };
}

[AttributeUsage(AttributeTargets.Class)]
public class EventHandlerContainerAttribute : Attribute
{
  public static IEnumerable<Type> GetTypes(Assembly asm)
    => asm.GetTypes()
      .Where(t => t.GetCustomAttribute(typeof(EventHandlerContainerAttribute)) != null);
}

[AttributeUsage(AttributeTargets.Method)]
public class EventHandlerAttribute : Attribute
{
  public readonly string EventName;

  public EventHandlerAttribute(string name)
  {
    EventName = name;
  }

  public static IEnumerable<MethodInfo> GetMethods(Assembly asm)
    => EventHandlerContainerAttribute.GetTypes(asm)
      .SelectMany(GetMethods);

  public static IEnumerable<MethodInfo> GetMethods(Type t)
    => t.GetMethods()
      .Where(m => m.GetCustomAttribute(typeof(EventHandlerAttribute)) != null);
}

public class IllegalEventHandlerException : Exception
{
  public MethodInfo Method;

  public IllegalEventHandlerException(MethodInfo m, string message) : base($"The method {m} is an invalid deserializer: {message}")
  {
    Method = m;
  }
}

public class OBSRequestFailedException : Exception
{
  public readonly int StatusCode;
  public readonly string Comment;

  public OBSRequestFailedException(int code, string comment) : base($"{code}: {comment}")
  {
    StatusCode = code;
    Comment = comment;
  }
}

public class OBSRequestTimedOutException : Exception
{
  public readonly string Guid;

  public OBSRequestTimedOutException(string guid) : base($"Request {guid} timed out")
  {
    Guid = guid;
  }
}