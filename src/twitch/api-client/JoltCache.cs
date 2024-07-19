using NodaTime;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;

namespace Nixill.Streaming.JoltBot.Twitch.Api;

public static class JoltCache
{
  static Instant Now => SystemClock.Instance.GetCurrentInstant();
  static readonly Duration OneHour = Duration.FromHours(1);
  static readonly Duration OneDay = Duration.FromDays(1);

  static ChannelInformation _OwnChannelInfo;
  static Instant _Update_OwnChannelInfo = Instant.MinValue;

  public static async Task<ChannelInformation> GetOwnChannelInfo()
  {
    if (Now > _Update_OwnChannelInfo + OneHour)
    {
      var response = await JoltApiClient.WithToken(
            api => api.Helix.Channels.GetChannelInformationAsync(
              JoltTwitchMain.Channel.UserId
            )
          );

      _OwnChannelInfo = response.Data.Where(i => i.BroadcasterId == JoltTwitchMain.Channel.UserId).First();
      _Update_OwnChannelInfo = Now;
    }

    return _OwnChannelInfo;
  }

  public static void SetOwnChannelInfo(ChannelInformation info)
  {
    _OwnChannelInfo = info;
    _Update_OwnChannelInfo = Now;
  }
}