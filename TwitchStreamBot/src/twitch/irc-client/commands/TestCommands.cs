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
}
