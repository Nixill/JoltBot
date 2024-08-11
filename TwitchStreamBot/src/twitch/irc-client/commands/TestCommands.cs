using Nixill.Streaming.JoltBot.Twitch.Api;
using Nixill.Utils;

namespace Nixill.Streaming.JoltBot.Twitch.Commands;

[CommandContainer]
public static class BasicCommands
{
  [Command("ping")]
  [AllowedUserGroups(TwitchUserGroup.Moderator)]
  public static Task PingCommand(BaseContext ctx)
    => ctx.ReplyAsync("Pong!");

  [Command("add")]
  public static Task AddCommand(BaseContext ctx, int left, params int[] right)
    => ctx.ReplyAsync($"{(right.Length <= 9 ? right.Prepend(left).SJoin(" + ")
      : $"The sum of your {right.Length + 1} numbers")} = {left + right.Sum()}");

  [Command("group")]
  public static async Task GroupCommand(BaseContext ctx)
    => await ctx.ReplyAsync((await JoltCache.GetUserGroup(ctx.UserId)).ToString());
}
