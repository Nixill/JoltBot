using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Nixill.Streaming.JoltBot.Twitch.Api;
using Nixill.Streaming.JoltBot.Twitch.Events;
using TwitchLib.Api;
using TwitchLib.Api.Auth;
using TwitchLib.Api.Core.Exceptions;

namespace Nixill.Streaming.JoltBot.Twitch;

public static class JoltTwitchMain
{
  internal static AuthInfo Channel;
  internal static AuthInfo Bot;

  static ILogger Logger = Log.Factory.CreateLogger(typeof(JoltTwitchMain));

  static JsonObject Obj;

  internal static string ClientId => (string)Obj["id"];
  internal static string ClientSecret => (string)Obj["secret"];

  public static async Task SetUpTwitchConnections()
  {
    Logger.LogInformation("Initializing Twitch connections.");

    Obj = (JsonObject)JsonNode.Parse(File.ReadAllText("data/twitch.json"));

    Channel = new(Obj["channel"]);
    Bot = new(Obj["bot"]);

    TwitchAPI api = new TwitchAPI(loggerFactory: Log.Factory);
    api.Settings.ClientId = (string)Obj["id"];
    api.Settings.Secret = (string)Obj["secret"];

    string channelToken = await GetToken(Channel, api);
    string botToken = await GetToken(Bot, api);

    Task botSetup = JoltChatBot.SetUp(Bot, Channel.Name);
    Task clientSetup = JoltApiClient.SetUp(Channel, api);
    JoltEventClient.SetUp();
  }

  internal static void SaveTwitchData()
  {
    File.WriteAllText("data/twitch.json", Obj.ToString());
  }

  public static async Task<string> GetToken(AuthInfo which, TwitchAPI api)
  {
    Logger.LogInformation($"Checking {which.Which} token.");
    ValidateAccessTokenResponse validity = await api.Auth.ValidateAccessTokenAsync(which.Token);

    if (validity == null)
    {
      Logger.LogWarning($"{which.Which} token is invalid, regenerating...");
      try
      {
        RefreshResponse answer = await api.Auth.RefreshAuthTokenAsync(which.Refresh, ClientSecret);
        which.Token = answer.AccessToken;
        which.Refresh = answer.RefreshToken;
        SaveTwitchData();

        validity = await api.Auth.ValidateAccessTokenAsync(which.Token);
      }
      catch (Exception ex)
      {
        Logger.LogCritical($"{which.Which} token regeneration failed. Stack trace: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
        throw new InvalidDataException("Token regeneration failed", ex);
      }
      if (validity == null)
      {
        throw new InvalidOperationException($"Bad {which.Which} token");
      }

      Logger.LogInformation($"Updated {which.Which} token.");
    }

    if (validity.Login.ToLowerInvariant() != which.Name.ToLowerInvariant())
    {
      Logger.LogError($"{which.Which} token username {validity.Login} does not match expected username {which.Name}");
      throw new InvalidDataException($"{which.Which} token username {validity.Login} does not match expected username {which.Name}");
    }

    Logger.LogInformation($"{which.Which} token scopes: {string.Join(", ", validity.Scopes.Order())}");
    Logger.LogInformation($"{which.Which} token username: {validity.Login}");

    return which.Token;
  }
}

public class AuthInfo
{
  public string Name
  {
    get => (string)Obj["name"];
    set => Obj["name"] = value;
  }
  public string Token
  {
    get => (string)Obj["token"];
    set => Obj["token"] = value;
  }
  public string Refresh
  {
    get => (string)Obj["refresh"];
    set => Obj["refresh"] = value;
  }
  public string UserId
  {
    get => (string)Obj["uid"];
    set => Obj["uid"] = value;
  }
  public string Which => Obj.GetPropertyName();

  JsonObject Obj;

  public AuthInfo(JsonNode input) : this((JsonObject)input) { }
  public AuthInfo(JsonObject input)
  {
    Obj = input;
  }
}
