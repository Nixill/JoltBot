using Microsoft.Extensions.Logging;
using TwitchLib.Api;
using TwitchLib.Api.Auth;
using TwitchLib.Api.Core.Exceptions;

namespace Nixill.Streaming.JoltBot.Twitch.Api;

public static class JoltApiClient
{
  static ILogger Logger = Log.Factory.CreateLogger("JoltApiClient");

  static AuthInfo ClientInfo;
  static TwitchAPI Api;

  public static Task SetUp(AuthInfo info, TwitchAPI api)
  {
    ClientInfo = info;
    Api = api;

    Api.Settings.AccessToken = ClientInfo.Token;

    return Task.CompletedTask;
  }

  internal static async Task<T> WithToken<T>(Func<TwitchAPI, Task<T>> call)
  {
    try
    {
      return await call(Api);
    }
    catch (TokenExpiredException)
    {
      await RefreshToken();
      return await call(Api);
    }
    catch (BadScopeException)
    {
      await RefreshToken();
      return await call(Api);
    }
  }

  internal static async Task WithToken(Func<TwitchAPI, Task> call)
  {
    try
    {
      await call(Api);
    }
    catch (TokenExpiredException)
    {
      await RefreshToken();
      await call(Api);
    }
    catch (BadScopeException)
    {
      await RefreshToken();
      await call(Api);
    }
  }

  static async Task RefreshToken()
  {
    Logger.LogWarning("Client API key expired, refreshing...");
    RefreshResponse answer = await Api.Auth.RefreshAuthTokenAsync(ClientInfo.Refresh, JoltTwitchMain.ClientSecret);
    ClientInfo.Token = Api.Settings.AccessToken = answer.AccessToken;
    ClientInfo.Refresh = answer.RefreshToken;
    JoltTwitchMain.SaveTwitchData();
  }
}