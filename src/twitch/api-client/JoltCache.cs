using Microsoft.Extensions.Logging;
using Nixill.Streaming.JoltBot.JSON;
using NodaTime;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;
using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;

namespace Nixill.Streaming.JoltBot.Twitch.Api;

public static class JoltCache
{
  static ILogger Logger = Log.Factory.CreateLogger(typeof(JoltCache));

  static Instant Now => SystemClock.Instance.GetCurrentInstant();
  static readonly Duration OneHour = Duration.FromHours(1);
  static readonly Duration OneDay = Duration.FromDays(1);

  static ChannelInformation _OwnChannelInfo;
  static Instant _Update_OwnChannelInfo = Instant.MinValue;

  public static async Task<ChannelInformation> GetOwnChannelInfo()
  {
    if (Now > _Update_OwnChannelInfo + OneHour)
    {
      Logger.LogInformation("Channel information expired.");
      await UpdateOwnChannelInfo();
    }

    return _OwnChannelInfo;
  }

  public static async Task UpdateOwnChannelInfo()
  {
    Logger.LogInformation("Refreshing channel information...");
    var response = await JoltApiClient.WithToken(
          api => api.Helix.Channels.GetChannelInformationAsync(
            TwitchJson.Channel.UserId
          )
        );

    _OwnChannelInfo = response.Data.Where(i => i.BroadcasterId == TwitchJson.Channel.UserId).First();
    _Update_OwnChannelInfo = Now;
  }
}

public class JChannelInformation : ChannelInformation
{
  public JChannelInformation(ChannelInformation info)
  {
    BroadcasterId = info.BroadcasterId;
    BroadcasterLanguage = info.BroadcasterLanguage;
    BroadcasterLogin = info.BroadcasterLogin;
    BroadcasterName = info.BroadcasterName;
    GameId = info.GameId;
    GameName = info.GameName;
    Title = info.Title;
    Delay = info.Delay;
    Tags = info.Tags;
  }

  public void Update(ChannelUpdate update)
  {
    if (BroadcasterId != update.BroadcasterUserId)
      throw new InvalidDataException
        ("The user ID of the JChannelInformation being updated must match the update payload's user ID.");

    BroadcasterLogin = update.BroadcasterUserLogin;
    BroadcasterName = update.BroadcasterUserName;
    GameId = update.CategoryId;
    GameName = update.CategoryName;
    Title = update.Title;
  }
}