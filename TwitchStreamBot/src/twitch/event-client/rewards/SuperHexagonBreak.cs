using System.Numerics;
using System.Text.RegularExpressions;
using Nixill.Streaming.JoltBot.Data;
using Nixill.Utils;
using NodaTime;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;

namespace Nixill.Streaming.JoltBot.Twitch.Events.Rewards;

[CommandContainer]
public static class SuperHexagonController
{
  static Instant Now => SystemClock.Instance.GetCurrentInstant();
  static CancellationTokenSource Timer = null;

  public static async Task SuperHexagonBreak(RewardContext ctx, SuperHexagonLevel level)
  {
    await ctx.MessageAsync("⚠️ Super Hexagon contains spinning and "
      + "flashing lights that may be problematic for some viewers. ⚠️");
    await ctx.MessageAsync("It usually lasts about 3 minutes. Nix's PB "
      + "is just under 5½ minutes.");

    Instant now = Now;

    SuperHexagonJson.Status = level;
    SuperHexagonJson.LastActivity = now;
    SuperHexagonJson.RedeemNum++;

    Timer?.Cancel();
    Timer = new();
    Task _ = CancelRedemptionAfterOneHour(ctx.RedemptionArgs.Id, now, Timer.Token);
  }

  static async Task CancelRedemptionAfterOneHour(string redemptionId, Instant startTime, CancellationToken token)
  {
    Instant now = Now;
    Instant halfHour = startTime + Duration.FromMinutes(30);
    Instant tenMinutes = halfHour + Duration.FromMinutes(20);
    Instant twoMinutes = tenMinutes + Duration.FromMinutes(8);
    Instant endOfTimer = twoMinutes + Duration.FromMinutes(2);

    try
    {
      if (now < halfHour)
      {
        await Task.Delay(Duration.Min(halfHour - now, Duration.FromMinutes(30)).ToTimeSpan(), token);
        await JoltChatBot.Chat("Hey, Nix! Did you forget about the Super Hexagon Break?");
      }

      if (now < tenMinutes)
      {
        await Task.Delay(Duration.Min(tenMinutes - now, Duration.FromMinutes(10)).ToTimeSpan(), token);
        await JoltChatBot.Chat("Nix, you've still got a Super Hexagon Break to do...");
      }

      if (now < twoMinutes)
      {
        await Task.Delay(Duration.Min(twoMinutes - now, Duration.FromMinutes(8)).ToTimeSpan(), token);
        await JoltChatBot.Chat("Nix, two minute warning on that Super Hexagon Break.");
      }

      if (now < endOfTimer)
      {
        await Task.Delay(Duration.Min(endOfTimer - now, Duration.FromMinutes(2)).ToTimeSpan(), token);
        await JoltChatBot.Chat("That's time! I'll just refund you that Super Hexagon Break redemption.");
      }

      // todo actually issue the refund and clean up the bot state (this
      // involves things I'm too tired to think about right now)
    }
    catch (TaskCanceledException) { }
  }
}

[RewardContainer]
public static class SuperHexagonRewards
{
  [ChannelPointsReward("SuperHexagon")]
  [DisableTemporarily]
  [SuperHexagon(SuperHexagonLevel.Hexagon)]
  public static async Task SuperHexagonBreakHexagon(RewardContext ctx)
    => await SuperHexagonController.SuperHexagonBreak(ctx, SuperHexagonLevel.Hexagon);

  [ChannelPointsReward("SuperHexagoner")]
  [DisableTemporarily]
  [SuperHexagon(SuperHexagonLevel.Hexagoner, SuperHexagonLevel.Hexagon)]
  public static async Task SuperHexagonBreakHexagoner(RewardContext ctx)
    => await SuperHexagonController.SuperHexagonBreak(ctx, SuperHexagonLevel.Hexagoner);

  [ChannelPointsReward("SuperHexagonest")]
  [DisableTemporarily]
  [SuperHexagon(SuperHexagonLevel.Hexagonest, SuperHexagonLevel.Hexagoner)]
  public static async Task SuperHexagonBreakHexagonest(RewardContext ctx)
    => await SuperHexagonController.SuperHexagonBreak(ctx, SuperHexagonLevel.Hexagonest);

  [ChannelPointsReward("SuperHexagonHyper")]
  [DisableTemporarily]
  [SuperHexagon(SuperHexagonLevel.HyperHexagon, SuperHexagonLevel.Hexagon)]
  public static async Task SuperHexagonBreakHyperHexagon(RewardContext ctx)
    => await SuperHexagonController.SuperHexagonBreak(ctx, SuperHexagonLevel.HyperHexagon);

  [ChannelPointsReward("SuperHexagonerHyper")]
  [DisableTemporarily]
  [SuperHexagon(SuperHexagonLevel.HyperHexagoner, SuperHexagonLevel.Hexagoner, SuperHexagonLevel.HyperHexagon)]
  public static async Task SuperHexagonBreakHyperHexagoner(RewardContext ctx)
    => await SuperHexagonController.SuperHexagonBreak(ctx, SuperHexagonLevel.HyperHexagoner);

  [ChannelPointsReward("SuperHexagonestHyper")]
  [DisableTemporarily]
  [SuperHexagon(SuperHexagonLevel.HyperHexagonest, SuperHexagonLevel.Hexagonest, SuperHexagonLevel.HyperHexagoner)]
  public static async Task SuperHexagonBreakHyperHexagonest(RewardContext ctx)
    => await SuperHexagonController.SuperHexagonBreak(ctx, SuperHexagonLevel.HyperHexagonest);
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
  None = 0,
  Hexagon = 1,
  Hexagoner = 2,
  Hexagonest = 3,
  HyperHexagon = 4,
  HyperHexagoner = 5,
  HyperHexagonest = 6
}