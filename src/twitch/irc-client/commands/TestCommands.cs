using Nixill.Utils;
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
}
