using System.Numerics;
using System.Text.RegularExpressions;
using Nixill.Utils;
using NodaTime;
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

public readonly partial struct SuperHexagonScore : IComparable<SuperHexagonScore>,
  IComparisonOperators<SuperHexagonScore, SuperHexagonScore, bool>,
  IComparisonOperators<SuperHexagonScore, Duration, bool>,
  IComparisonOperators<SuperHexagonScore, double, bool>,
  IAdditionOperators<SuperHexagonScore, SuperHexagonScore, SuperHexagonScore>
{
  public int Frames { get; init; }

  public int Seconds => Frames / 60;
  public int PartialFrames => Frames % 60;

  public decimal DecimalSeconds => Frames / 60m;
  public double DoubleSeconds => Frames / 60d;

  public Duration Duration => Duration.FromSeconds(DoubleSeconds);

  static readonly Regex TimeFormat = TimeFormatGenerator();

  public static SuperHexagonScore Parse(string input)
  {
    if (!TimeFormat.TryMatch(input, out Match match))
    {
      throw new FormatException("Not a valid Super Hexagon runtime.");
    }

    int frames = int.Parse(match.Groups[2].Value);
    if (match.Groups[1].Success) frames += int.Parse(match.Groups[1].Value) * 60;

    return new SuperHexagonScore { Frames = frames };
  }

  [GeneratedRegex(@"(?:(\d+):)?(\d\d?)")]
  private static partial Regex TimeFormatGenerator();

  public int CompareTo(SuperHexagonScore other)
    => Frames.CompareTo(other.Frames);

  public static bool operator >(SuperHexagonScore left, SuperHexagonScore right) => left.Frames > right.Frames;
  public static bool operator >=(SuperHexagonScore left, SuperHexagonScore right) => left.Frames >= right.Frames;
  public static bool operator <(SuperHexagonScore left, SuperHexagonScore right) => left.Frames < right.Frames;
  public static bool operator <=(SuperHexagonScore left, SuperHexagonScore right) => left.Frames <= right.Frames;
  public static bool operator ==(SuperHexagonScore left, SuperHexagonScore right) => left.Frames == right.Frames;
  public static bool operator !=(SuperHexagonScore left, SuperHexagonScore right) => left.Frames != right.Frames;

  public static bool operator >(SuperHexagonScore left, Duration right) => left.Duration > right;
  public static bool operator >=(SuperHexagonScore left, Duration right) => left.Duration >= right;
  public static bool operator <(SuperHexagonScore left, Duration right) => left.Duration < right;
  public static bool operator <=(SuperHexagonScore left, Duration right) => left.Duration <= right;
  public static bool operator ==(SuperHexagonScore left, Duration right) => left.Duration == right;
  public static bool operator !=(SuperHexagonScore left, Duration right) => left.Duration != right;

  public static bool operator >(SuperHexagonScore left, double right) => left.DoubleSeconds > right;
  public static bool operator >=(SuperHexagonScore left, double right) => left.DoubleSeconds >= right;
  public static bool operator <(SuperHexagonScore left, double right) => left.DoubleSeconds < right;
  public static bool operator <=(SuperHexagonScore left, double right) => left.DoubleSeconds <= right;
  public static bool operator ==(SuperHexagonScore left, double right) => left.DoubleSeconds == right;
  public static bool operator !=(SuperHexagonScore left, double right) => left.DoubleSeconds != right;

  public static SuperHexagonScore operator +(SuperHexagonScore left, SuperHexagonScore right) => new SuperHexagonScore { Frames = left.Frames + right.Frames };

  public override bool Equals(object obj)
    => Frames == (obj as SuperHexagonScore?)?.Frames;

  public override int GetHashCode()
    => Frames.GetHashCode();
}

public enum SuperHexagonLevel
{
  Hexagon = 1,
  Hexagoner = 2,
  Hexagonest = 3,
  HyperHexagon = 4,
  HyperHexagoner = 5,
  HyperHexagonest = 6
}