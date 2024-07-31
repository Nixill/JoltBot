using System.Text.Json.Nodes;
using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.OBS;
using Nixill.Utils;
using NodaTime;
using NodaTime.Text;
using NodaTime.TimeZones;
using TwitchLib.Client.Events;
using Args = Nixill.Streaming.JoltBot.Twitch.CommandContext;

namespace Nixill.Streaming.JoltBot.Twitch.Commands;

[CommandContainer]
public static class BasicCommands
{
  [Command("ping")]
  [AllowedGroups(TwitchUserGroup.Moderator)]
  public static Task PingCommand(Args ev)
    => ev.ReplyAsync("Pong!");

  [Command("add")]
  public static Task AddCommand(Args ev, int left, params int[] right)
    => ev.ReplyAsync($"{(right.Length <= 9 ? right.Prepend(left).SJoin(" + ")
      : $"The sum of your {right.Length + 1} numbers")} = {left + right.Sum()}");

  [Command("group")]
  public static async Task GroupCommand(Args ev)
    => await ev.ReplyAsync((await ev.ChatCommandArgs.GetUserGroup(true, true)).ToString());

  static ZonedDateTimePattern ZDTPattern = ZonedDateTimePattern.CreateWithInvariantCulture("ddd HH:mm:ss", null);
  static DateTimeZone DefaultZone = BclDateTimeZone.ForSystemDefault();

  [Command("time")]
  public static async Task GetClockTime(Args ev)
  {
    OBSRequest<InputSettingsResult> request = OBSRequests.Inputs.GetInputSettings("txt_Clock");
    InputSettingsResult response = await JoltOBSClient.Client.SendRequest(request);
    string returnedTime = (string)(response.InputSettings["text"]);

    await ev.ReplyAsync(returnedTime);
  }

  [Command("ad start")]
  [AllowedGroups(TwitchUserGroup.Moderator)]
  public static async Task RunAnAd(Args ev, int length = 180)
  {
    if (AdManager.TryStartAd(length) == true)
    {
      await ev.ReplyAsync("Ads will run in one minute!");
      return;
    }

    Task _ = AdManager.RunAdAfterCountdown();
    AdManager.NextAdDuration = 180;

    await ev.ReplyAsync("Ads will run in one minute!");
  }

  [Command("ad stop", "ad cancel")]
  [AllowedGroups(TwitchUserGroup.Moderator)]
  public static async Task StopAnAd(Args ev)
  {
    if (AdManager.TryStopAd())
    {
      Task _ = AdManager.StopAdCountdown();
      await ev.ReplyAsync("Stopped the upcoming ad break!");
    }
    else
    {
      await ev.ReplyAsync("There is no upcoming ad break! (Or it's already running; this cannot be cancelled.)");
    }
  }
}
