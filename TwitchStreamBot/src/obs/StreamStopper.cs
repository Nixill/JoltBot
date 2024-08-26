using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.Discord;
using Nixill.Streaming.JoltBot.Data;
using Nixill.Streaming.JoltBot.Twitch;
using Nixill.Streaming.JoltBot.Twitch.Api;
using NodaTime;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;

namespace Nixill.Streaming.JoltBot.OBS;

public static class StreamStopper
{
  static Instant LastWarning = Instant.MinValue;
  static Instant Now => SystemClock.Instance.GetCurrentInstant();
  static Duration HalfHour => Duration.FromMinutes(30);

  internal static bool EndingForStreamTitle = false;

  public static async Task CheckUpdatedOnStartup()
  {
    var channelInfo = await JoltApiClient
      .WithToken(async (api, id) => (await api.Helix.Channels.GetChannelInformationAsync(id))
        .Data.Where(info => info.BroadcasterId == id).First()
      );
    (Instant lastUpdate, string title, string game, string[] tags) = MemoryJson.Stopper.AllInfo;

    if (title != channelInfo.Title || game != channelInfo.GameName || !tags.Order().SequenceEqual(channelInfo.Tags.Order()))
    {
      await HandleStreamUpdate(channelInfo);
    }
  }

  public static async Task HandleStreamUpdate(ChannelInformation info)
  {
    await Task.Delay(0);

    Instant now = Now;
    MemoryJson.Stopper.AllInfo = (
      now,
      info.Title,
      info.GameName,
      info.Tags
    );

    LastWarning = now;

    // Check stream title validity
    if (!GamesCsv.IsValidTitle(info.GameName, info.Title))
    {
      await WarnInvalidTitle();
    }
  }

  public static async Task HandleStreamStart()
  {
    await Task.Delay(0);

    Instant now = Now;

    if (MemoryJson.Stopper.LastChanged < now - HalfHour && LastWarning < now - HalfHour)
    {
      EndingForStreamTitle = true;

      AdManager.TryStopAd();
      OBSRequests.Stream.StopStream().SendWithoutWaiting();

      var sendToDiscord = WebhookClient.SendPingingMessage("StreamAlerts", "You forgot to update stream info!");
      var sendToTwitch = JoltChatBot.Chat("Stream is stopping because Nix forgot to update stream title!");
    }
  }

  public static async Task WarnInvalidTitle()
  {
    var getStreamStatus = OBSRequests.Stream.GetStreamStatus().Send();
    var sendToDiscord = WebhookClient.SendPingingMessage("StreamAlerts",
      "Stream title and game don't match!");
    if ((await getStreamStatus).Active)
    {
      await JoltChatBot.Chat("Nix, your stream title and game don't match!");
    }
  }
}