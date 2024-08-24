using Microsoft.Extensions.Logging;
using Nixill.Streaming.JoltBot.Data;
using TwitchLib.Api;
using TwitchLib.Api.Auth;
using TwitchLib.Api.Core.Exceptions;

namespace Nixill.Streaming.JoltBot.Twitch.Api;

public static class JoltApiClient
{
  static ILogger Logger = Log.Factory.CreateLogger(typeof(JoltApiClient));

  static AuthInfo ClientInfo;
  static TwitchAPI Api;

  public static Task SetUp(AuthInfo info, TwitchAPI api)
  {
    ClientInfo = info;
    Api = api;

    Api.Settings.AccessToken = ClientInfo.Token;

    return Task.CompletedTask;
  }

  internal static Task<T> WithToken<T>(Func<TwitchAPI, Task<T>> call) => WithToken((api, _) => call(api));
  internal static async Task<T> WithToken<T>(Func<TwitchAPI, string, Task<T>> call)
  {
    try
    {
      int i = 0;
      while (true)
        try
        {
          return await call(Api, TwitchJson.Channel.UserId);
        }
        catch (InternalServerErrorException) when (i < 5)
        {
          await Task.Delay(5);
          i++;
        }
    }
    catch (TokenExpiredException)
    {
      await RefreshToken();
      return await call(Api, TwitchJson.Channel.UserId);
    }
    catch (BadScopeException)
    {
      await RefreshToken();
      return await call(Api, TwitchJson.Channel.UserId);
    }
  }

  internal static Task WithToken<T>(Func<TwitchAPI, Task> call) => WithToken((api, _) => call(api));
  internal static async Task WithToken(Func<TwitchAPI, string, Task> call)
  {
    try
    {
      await call(Api, TwitchJson.Channel.UserId);
    }
    catch (TokenExpiredException)
    {
      await RefreshToken();
      await call(Api, TwitchJson.Channel.UserId);
    }
    catch (BadScopeException)
    {
      await RefreshToken();
      await call(Api, TwitchJson.Channel.UserId);
    }
  }

  static async Task RefreshToken()
  {
    Logger.LogWarning("Client API key expired, refreshing...");
    RefreshResponse answer = await Api.Auth.RefreshAuthTokenAsync(ClientInfo.Refresh, TwitchJson.Secret);
    ClientInfo.Token = Api.Settings.AccessToken = answer.AccessToken;
    ClientInfo.Refresh = answer.RefreshToken;
    TwitchJson.Save();
  }
}