using Microsoft.Extensions.Logging;
using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.Data;
using Nixill.Streaming.JoltBot.Games.UFO50;
using Websocket.Client.Exceptions;

namespace Nixill.Streaming.JoltBot.OBS;

public static class JoltOBSClient
{
  const int GlobalTimeout = 3600;
  internal static OBSClient Client;
  static readonly ILogger Logger = Log.Factory.CreateLogger(typeof(JoltOBSClient));

  static readonly TimeSpan ReconnectTimeout = TimeSpan.FromSeconds(5);

  public static bool IsConnecting { get; private set; } = false;
  public static bool IsConnected => Client.IsConnected;
  public static bool IsIdentified => Client.IsIdentified;

  public static async Task SetUp()
  {
    Client = new(OBSJson.Server.IP, OBSJson.Server.Port, OBSJson.Server.Password, loggerFactory: Log.Factory);

    Client.Events.Outputs.StreamStarted += (s, e) => Task.Run(() => JoltOBSEventHandlers.StreamStarted(s, e));
    Client.Events.Outputs.StreamStopped += (s, e) => Task.Run(() => JoltOBSEventHandlers.StreamStopped(s, e));
    Client.Events.MediaInputs.MediaInputPlaybackEnded += (s, e) => Task.Run(() => BingoMusicController.MusicPlaybackEnded(s, e));

    Client.Disconnected += Reconnect;

    await ConnectAsync();
  }

  private static async Task ConnectAsync()
  {
    IsConnecting = true;
    while (!Client.IsConnected)
    {
      try
      {
        await Client.ConnectAsync();
      }
      catch (Exception)
      {
        Logger.LogError("Failed to connect to OBS websocket server.");
      }
      await Task.Delay(ReconnectTimeout);
    }
    IsConnecting = false;
  }

  private static void Reconnect(object sender, OBSDisconnectedArgs e)
  {
    if (!IsConnecting)
    {
      Client.Dispose();
      Task _ = SetUp();
    }
  }

  public static Task<T> Send<T>(this OBSRequest<T> request, int timeout = GlobalTimeout) where T : OBSRequestResult
    => Client.SendRequest(request, timeout);

  public static Task Send(this OBSVoidRequest request, int timeout = GlobalTimeout)
    => Client.SendRequest(request, timeout);

  public static void SendWithoutWaiting(this OBSRequest request)
    => Client.SendRequestWithoutWaiting(request);

  public static Task<OBSRequestBatchResult> Send(this OBSRequestBatch requests, int timeout = GlobalTimeout)
    => Client.SendBatchRequest(requests, timeout: timeout);

  public static void SendWithoutWaiting(this OBSRequestBatch requests)
    => Client.SendBatchRequestWithoutWaiting(requests);
}