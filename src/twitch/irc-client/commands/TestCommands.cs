using System.Text.Json.Nodes;
using Nixill.Streaming.JoltBot.OBS;
using Nixill.Utils;
using NodaTime;
using NodaTime.Text;
using NodaTime.TimeZones;
using TwitchLib.Client.Events;
using Args = TwitchLib.Client.Events.OnChatCommandReceivedArgs;

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
    => await ev.ReplyAsync((await ev.GetUserGroup(true, true)).ToString());

  static ZonedDateTimePattern ZDTPattern = ZonedDateTimePattern.CreateWithInvariantCulture("ddd HH:mm:ss", null);
  static DateTimeZone DefaultZone = BclDateTimeZone.ForSystemDefault();

  [Command("time")]
  public static async Task TimeCommand(Args ev)
  {
    try
    {
      var response = await OBSClient.SendRequest("SetInputSettings", new JsonObject
      {
        ["inputName"] = "txt_Clock",
        ["inputSettings"] = new JsonObject
        {
          ["text"] = ZDTPattern.Format(SystemClock.Instance.GetCurrentInstant().InZone(DefaultZone))
        }
      });
      await ev.ReplyAsync("Success!");
    }
    catch (Exception e)
    {
      await ev.ReplyAsync($"Failed: {e.GetType().Name} thrown");
    }
  }
}
