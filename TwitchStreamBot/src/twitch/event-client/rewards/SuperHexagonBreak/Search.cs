using System.Text.RegularExpressions;
using Nixill.Streaming.JoltBot.Data;
using Nixill.Utils;
using NodaTime;

namespace Nixill.Streaming.JoltBot.Twitch.Events.Rewards;

public static class SuperHexagonSearchParser
{
  static readonly Regex partialDate = new(@"^(\d{4})(?:-(\d\d)(?:-(\d\d)))$");

  public SuperHexagonSearchQuery ParseQuery(CommandContext ctx, IList<string> words)
  {
    SuperHexagonSearchQuery query = new();
    DirectionalModifier modifier = DirectionalModifier.None;

    while (words.Count > 0)
    {
      string word = words.Pop().ToLower();

      if (partialDate.TryMatch(word, out Match mtc))
      {
        int year = int.Parse(mtc.Groups[1].Value);
        int? month = mtc.Groups[2].Success ? int.Parse(mtc.Groups[2].Value) : null;
        int? day = mtc.Groups[3].Success ? int.Parse(mtc.Groups[3].Value) : null;

        ParseDate(query, modifier, year, month, day);
        continue;
      }
      else if (word == "redemption")
      {
        ParseRedemption(query, modifier, words, true);
        continue;
      }
      else if (word == "attempt")
      {
        ParseAttempt(query, modifier, words, false);
        continue;
      }
    }
  }

  static void ParseDate(SuperHexagonSearchQuery query, DirectionalModifier modifier, int year, int? month, int? day)
  {
    LocalDate lowestDate = new LocalDate(year, month ?? 1, day ?? 1);
    int lowestAttempt = SuperHexagonCSVs.Redemptions
      .Where(r => r.Date >= lowestDate)
      .MinBy(r => r.RedemptionID)
      .GetAttempts()
      .Min(a => a.AttemptId);

    LocalDate highestDate = day != null
      ? new LocalDate(year, month.Value, day.Value)
      : new LocalDate(year, month ?? 12, CalendarSystem.Gregorian.GetDaysInMonth(year, month ?? 12));
    int highestAttempt = SuperHexagonCSVs.Redemptions
      .Where(r => r.Date <= highestDate)
      .MaxBy(r => r.RedemptionID)
      .GetAttempts()
      .Max(a => a.AttemptId);

    query.LowestAttempt = int.Max(query.LowestAttempt, modifier switch
    {
      DirectionalModifier.None or DirectionalModifier.GreaterOrEqual => lowestAttempt,
      DirectionalModifier.GreaterThan => highestAttempt + 1,
      _ => 0
    });

    query.HighestAttempt = int.Min(query.HighestAttempt, modifier switch
    {
      DirectionalModifier.None or DirectionalModifier.LessOrEqual => highestAttempt,
      DirectionalModifier.LessThan => lowestAttempt - 1,
      _ => int.MaxValue
    });
  }

  static void ParseRedemption(SuperHexagonSearchQuery query, DirectionalModifier modifier, IList<string> words,
    bool isRedemption)
  {

  }
}

public class SuperHexagonSearchQuery
{
  public int LowestAttempt { get; internal set; } = 1;
  public int HighestAttempt { get; internal set; } = int.MaxValue;

  public SuperHexagonScore LowestScore { get; internal set; } = SuperHexagonScore.Zero;
  public SuperHexagonScore HighestScore { get; internal set; } = SuperHexagonScore.MaxValue;

  public string RedeemerUsername { get; internal set; } = null;
  public SuperHexagonLevel[] Levels { get; internal set; } = null;
}

public enum DirectionalModifier
{
  LessThan = -2,
  LessOrEqual = -1,
  None = 0,
  GreaterOrEqual = 1,
  GreaterThan = 2
}