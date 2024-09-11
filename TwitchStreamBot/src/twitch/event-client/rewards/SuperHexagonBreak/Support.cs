using System.Numerics;
using System.Text.RegularExpressions;
using Nixill.Utils;
using NodaTime;
using TwitchLib.Api.Helix.Models.Moderation.ShieldModeStatus;

namespace Nixill.Streaming.JoltBot.Twitch.Events.Rewards;


public readonly partial struct SuperHexagonScore : IComparable<SuperHexagonScore>,
  IComparisonOperators<SuperHexagonScore, SuperHexagonScore, bool>,
  IComparisonOperators<SuperHexagonScore, Duration, bool>,
  IComparisonOperators<SuperHexagonScore, double, bool>,
  IAdditionOperators<SuperHexagonScore, SuperHexagonScore, SuperHexagonScore>,
  ISubtractionOperators<SuperHexagonScore, SuperHexagonScore, SuperHexagonScore>,
  IAdditiveIdentity<SuperHexagonScore, SuperHexagonScore>
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

  public static bool operator >(SuperHexagonScore left, int right) => left.Frames > right;
  public static bool operator >=(SuperHexagonScore left, int right) => left.Frames >= right;
  public static bool operator <(SuperHexagonScore left, int right) => left.Frames < right;
  public static bool operator <=(SuperHexagonScore left, int right) => left.Frames <= right;
  public static bool operator ==(SuperHexagonScore left, int right) => left.Frames == right;
  public static bool operator !=(SuperHexagonScore left, int right) => left.Frames != right;

  public static SuperHexagonScore operator +(SuperHexagonScore left, SuperHexagonScore right)
    => new SuperHexagonScore { Frames = left.Frames + right.Frames };
  public static SuperHexagonScore operator -(SuperHexagonScore left, SuperHexagonScore right)
    => new SuperHexagonScore { Frames = left.Frames - right.Frames };

  public static readonly SuperHexagonScore Zero = new() { Frames = 0 };
  public static SuperHexagonScore AdditiveIdentity => Zero;
  public static readonly SuperHexagonScore Win = new() { Frames = 3600 };
  public static readonly SuperHexagonScore Lose = new() { Frames = 7200 };

  public override bool Equals(object obj)
    => Frames == (obj as SuperHexagonScore?)?.Frames;

  public override int GetHashCode()
    => Frames.GetHashCode();

  public override string ToString()
    => $"{Seconds}:{PartialFrames:D2}";
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

public enum SuperHexagonStatus
{
  /// <summary>
  /// There is no Super Hexagon Break redeem active.
  /// </summary>
  None = 0,
  /// <summary>
  /// A Super Hexagon Break has been redeemed but not acknowledged yet.
  /// </summary>
  Waiting = 1,
  /// <summary>
  /// A Super Hexagon Break has been acknowledged, but not completed yet.
  /// </summary>
  Active = 2,
  /// <summary>
  /// A Super Hexagon Break has been completed, but it is not time for a
  /// new one yet.
  /// </summary>
  Cooldown = 3
}