using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Nixill.Streaming.JoltBot.Data;
using Nixill.Streaming.JoltBot.Twitch.Api;
using Nixill.Streaming.JoltBot.Twitch.Events;
using Nixill.Streaming.JoltBot.Twitch.Events.Rewards;
using TwitchLib.Api;
using TwitchLib.Api.Auth;
using TwitchLib.Api.Core.Exceptions;

namespace Nixill.Streaming.JoltBot.Twitch;

public static class JoltTwitchMain
{
  static ILogger Logger = Log.Factory.CreateLogger(typeof(JoltTwitchMain));

  public static async Task SetUpTwitchConnections()
  {
    Logger.LogInformation("Initializing Twitch connections.");

    TwitchAPI api = new TwitchAPI(loggerFactory: Log.Factory);
    api.Settings.ClientId = TwitchJson.ID;
    api.Settings.Secret = TwitchJson.Secret;

    string channelToken = await GetToken(TwitchJson.Channel, api);
    string botToken = await GetToken(TwitchJson.Bot, api);

    Task botSetup = JoltChatBot.SetUp(TwitchJson.Bot, TwitchJson.Channel.Name);
    Task clientSetup = JoltApiClient.SetUp(TwitchJson.Channel, api);
    Task eventSetup = Task.Run(JoltEventClient.SetUp);
    Task rewardSetup = JoltRewardDispatch.Register();

    Task shSetup = SuperHexagonController.Startup();
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
        RefreshResponse answer = await api.Auth.RefreshAuthTokenAsync(which.Refresh, TwitchJson.Secret);
        which.Token = answer.AccessToken;
        which.Refresh = answer.RefreshToken;
        TwitchJson.Save();

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
