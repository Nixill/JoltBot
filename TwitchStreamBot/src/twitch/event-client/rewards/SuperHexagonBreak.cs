using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;

namespace Nixill.Streaming.JoltBot.Twitch.Events.Rewards;

[CommandContainer]
public static class SuperHexagonController
{
  internal static HashSet<string> PlayedRounds = [];
  internal static HashSet<string> WonRounds = [];

  internal static string CurrentRound = null;

  public static async Task SuperHexagonBreak(RewardContext ctx, string level)
  {
    await ctx.MessageAsync("⚠️ Super Hexagon contains spinning and "
      + "flashing lights that may be problematic for some viewers. ⚠️");
    await ctx.MessageAsync("It usually lasts about 3 minutes. Nix's PB "
      + "is just under 5½ minutes.");

    CurrentRound = level;
  }
}

[RewardContainer]
public static class SuperHexagonRewards
{
  [ChannelPointsReward("SuperHexagon")]
  [DisableTemporarily]
  [SuperHexagon("hexagon")]
  public static async Task SuperHexagonBreakHexagon(RewardContext ctx)
    => await SuperHexagonController.SuperHexagonBreak(ctx, "hexagon");

  [ChannelPointsReward("SuperHexagoner")]
  [DisableTemporarily]
  [SuperHexagon("hexagoner", "hexagon")]
  public static async Task SuperHexagonBreakHexagoner(RewardContext ctx)
    => await SuperHexagonController.SuperHexagonBreak(ctx, "hexagoner");

  [ChannelPointsReward("SuperHexagonest")]
  [DisableTemporarily]
  [SuperHexagon("hexagonest", "hexagoner")]
  public static async Task SuperHexagonBreakHexagonest(RewardContext ctx)
    => await SuperHexagonController.SuperHexagonBreak(ctx, "hexagonest");

  [ChannelPointsReward("SuperHexagonHyper")]
  [DisableTemporarily]
  [SuperHexagon("hyper-hexagon", "hexagon")]
  public static async Task SuperHexagonBreakHyperHexagon(RewardContext ctx)
    => await SuperHexagonController.SuperHexagonBreak(ctx, "hyper-hexagon");

  [ChannelPointsReward("SuperHexagonerHyper")]
  [DisableTemporarily]
  [SuperHexagon("hyper-hexagoner", "hexagoner", "hyper-hexagon")]
  public static async Task SuperHexagonBreakHyperHexagoner(RewardContext ctx)
    => await SuperHexagonController.SuperHexagonBreak(ctx, "hyper-hexagoner");

  [ChannelPointsReward("SuperHexagonestHyper")]
  [DisableTemporarily]
  [SuperHexagon("hyper-hexagonest", "hexagonest", "hyper-hexagoner")]
  public static async Task SuperHexagonBreakHyperHexagonest(RewardContext ctx)
    => await SuperHexagonController.SuperHexagonBreak(ctx, "hyper-hexagonest");
}

public class SuperHexagonAttribute(string lvl, params string[] prereqs) : LimitAttribute
{
  public string Level = lvl;
  public string[] Prerequisites = prereqs;

  protected override Task<bool> ConditionCheck(BaseContext ctx, ChannelInformation info)
  {
    if (info.Tags.Any(t => t.Equals("speedrun", StringComparison.InvariantCultureIgnoreCase))) return Task.FromResult(false);
    if (Prerequisites.Any(p => !SuperHexagonController.PlayedRounds.Contains(p))) return Task.FromResult(false);
    if (SuperHexagonController.PlayedRounds.Contains(Level)) return Task.FromResult(false);

    return Task.FromResult(true);
  }
}