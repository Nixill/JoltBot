namespace Nixill.Streaming.JoltBot.Twitch.Events.Rewards;

[RewardContainer]
public static class SuperHexagonContainer
{
  [ChannelPointsReward("SuperHexagon")]
  [AllowedWithTag("Speedrun", Invert = true)]
  public static async Task SuperHexagonBreak(RewardContext ctx)
  {
    await ctx.MessageAsync("⚠️ Super Hexagon contains spinning and "
      + "flashing lights that may be problematic for some viewers. ⚠️");
    await ctx.MessageAsync("It usually lasts about 3 minutes. Nix's PB "
      + "is just under 5½ minutes.");
  }
}