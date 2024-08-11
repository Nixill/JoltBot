using Nixill.Streaming.JoltBot.Twitch.Api;

namespace Nixill.Streaming.JoltBot.Twitch.Commands;

[CommandContainer]
public static class SillyCommands
{
  [Command("ban")]
  public static async Task BanCommand(BaseContext ctx, [LongText] string name)
  {
    // The less-than in the following if condition is unorthodox, and is a
    // holdover from when a user could only have one group, but I guess
    // it's valid now anyway - basically saying that the user's *highest*
    // group is less than the Moderator level.
    if (!(await JoltCache.GetUserGroup(ctx.UserId)).HasFlag(TwitchUserGroup.Moderator) && Random.Shared.NextDouble() < 0.05)
    {
      name = ctx.UserName;
    }

    await ctx.ReplyAsync($"You're banned, {name}!");
  }

  [Command("coinflip", "coin")]
  public static async Task CoinFlipCommand(BaseContext ctx)
  {
    await ctx.ReplyAsync("Flipping a coin...");
    await Task.Delay(1000);
    await ctx.MessageAsync($"It landed {(Random.Shared.Next(2) == 1 ? "heads" : "tails")}.");
  }

  [Command("countdown")]
  [AllowedUserGroups(TwitchUserGroup.Moderator)]
  public static async Task CountdownCommand(BaseContext ctx)
  {
    await ctx.ReplyAsync("Ready?");
    await Task.Delay(1000);
    await ctx.MessageAsync("5...");
    await Task.Delay(1000);
    await ctx.MessageAsync("4...");
    await Task.Delay(1000);
    await ctx.MessageAsync("3...");
    await Task.Delay(1000);
    await ctx.MessageAsync("2...");
    await Task.Delay(1000);
    await ctx.MessageAsync("1...");
    await Task.Delay(1000);
    await ctx.MessageAsync("Go!");
  }
}