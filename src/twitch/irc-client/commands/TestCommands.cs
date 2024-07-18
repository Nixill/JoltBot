using Nixill.Utils;
using TwitchLib.Client.Events;

namespace Nixill.Streaming.JoltBot.Twitch.Commands;

[CommandContainer]
public static class BasicCommands
{
  [Command("ping")]
  [AllowAtLeast(TwitchUserGroup.Moderator)]
  public static Task PingCommand(OnChatCommandReceivedArgs ev)
    => ev.ReplyAsync("Pong!");

  [Command("add")]
  public static Task AddCommand(OnChatCommandReceivedArgs ev, int left, params int[] right)
    => ev.ReplyAsync($"{(right.Length <= 9 ? right.Prepend(left).SJoin(" + ")
      : $"The sum of your {right.Length + 1} numbers")} = {left + right.Sum()}");
}
