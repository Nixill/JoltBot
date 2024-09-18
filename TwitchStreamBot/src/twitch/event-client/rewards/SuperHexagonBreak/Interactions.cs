using Nixill.Streaming.JoltBot.Data;
using Nixill.Utils;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;

namespace Nixill.Streaming.JoltBot.Twitch.Events.Rewards;

[RewardContainer]
public static class SuperHexagonRewards
{
  [ChannelPointsReward("SuperHexagon.Hexagon")]
  // [DisableTemporarily]
  [SuperHexagon(SuperHexagonLevel.Hexagon)]
  public static async Task SuperHexagonBreakHexagon(RewardContext ctx)
    => await SuperHexagonController.SuperHexagonBreak(ctx, SuperHexagonLevel.Hexagon);

  [ChannelPointsReward("SuperHexagon.Hexagoner")]
  // [DisableTemporarily]
  [SuperHexagon(SuperHexagonLevel.Hexagoner, SuperHexagonLevel.Hexagon)]
  public static async Task SuperHexagonBreakHexagoner(RewardContext ctx)
    => await SuperHexagonController.SuperHexagonBreak(ctx, SuperHexagonLevel.Hexagoner);

  [ChannelPointsReward("SuperHexagon.Hexagonest")]
  // [DisableTemporarily]
  [SuperHexagon(SuperHexagonLevel.Hexagonest, SuperHexagonLevel.Hexagoner)]
  public static async Task SuperHexagonBreakHexagonest(RewardContext ctx)
    => await SuperHexagonController.SuperHexagonBreak(ctx, SuperHexagonLevel.Hexagonest);

  [ChannelPointsReward("SuperHexagon.HyperHexagon")]
  // [DisableTemporarily]
  [SuperHexagon(SuperHexagonLevel.HyperHexagon, SuperHexagonLevel.Hexagon)]
  public static async Task SuperHexagonBreakHyperHexagon(RewardContext ctx)
    => await SuperHexagonController.SuperHexagonBreak(ctx, SuperHexagonLevel.HyperHexagon);

  [ChannelPointsReward("SuperHexagon.HyperHexagoner")]
  // [DisableTemporarily]
  [SuperHexagon(SuperHexagonLevel.HyperHexagoner, SuperHexagonLevel.Hexagoner, SuperHexagonLevel.HyperHexagon)]
  public static async Task SuperHexagonBreakHyperHexagoner(RewardContext ctx)
    => await SuperHexagonController.SuperHexagonBreak(ctx, SuperHexagonLevel.HyperHexagoner);

  [ChannelPointsReward("SuperHexagon.HyperHexagonest")]
  // [DisableTemporarily]
  [SuperHexagon(SuperHexagonLevel.HyperHexagonest, SuperHexagonLevel.Hexagonest, SuperHexagonLevel.HyperHexagoner)]
  public static async Task SuperHexagonBreakHyperHexagonest(RewardContext ctx)
    => await SuperHexagonController.SuperHexagonBreak(ctx, SuperHexagonLevel.HyperHexagonest);
}

[CommandContainer]
[DeserializerContainer]
public static class SuperHexagonCommands
{
  [Command("shb ack", "hex ack")]
  [AllowedUserGroups(TwitchUserGroup.Moderator)]
  // [DisableTemporarily]
  public static async Task SHAcknowledge(BaseContext ctx)
    => await SuperHexagonController.Acknowledge(ctx);

  [Command("shb score", "hex score")]
  [AllowedUserGroups(TwitchUserGroup.Moderator)]
  // [DisableTemporarily]
  public static async Task SHScore(CommandContext ctx, SuperHexagonScore score)
    => await SuperHexagonController.Score(ctx, score);

  [Deserializer]
  public static SuperHexagonScore ScoreDeserializer(IList<string> input, bool isLongText)
    => SuperHexagonScore.Parse(input.Pop());
}

public class SuperHexagonAttribute(SuperHexagonLevel lvl, params SuperHexagonLevel[] prereqs) : LimitAttribute
{
  public SuperHexagonLevel Level = lvl;
  public SuperHexagonLevel[] Prerequisites = prereqs;

  protected override Task<bool> ConditionCheck(BaseContext ctx, ChannelInformation info)
  {
    if (info.Tags.Any(t => t.Equals("speedrun", StringComparison.InvariantCultureIgnoreCase))) return Task.FromResult(false);
    if (Prerequisites.Any(p => !SuperHexagonJson.Played.Contains(p))) return Task.FromResult(false);
    if (SuperHexagonJson.Played.Contains(Level)) return Task.FromResult(false);

    return Task.FromResult(true);
  }
}
