using Nixill.Utils;
using NodaTime;

namespace Nixill.Streaming.JoltBot.Twitch.Events.Rewards;

public static class SuperHexagonSearchParser
{
  public SuperHexagonSearchQuery ParseQuery(CommandContext ctx, IList<string> words)
  {
    SuperHexagonSearchQuery query = new();

    while (words.Count > 0)
    {
      string word = words.Pop();

      
    }
  }
}

public class SuperHexagonSearchQuery
{
  public LocalDate EarliestDate { get; internal set; } = LocalDate.MinIsoValue;
  public LocalDate LatestDate { get; internal set; } = LocalDate.MaxIsoValue;
  public int LowestRedemption { get; internal set; } = 1;
  public int HighestRedemption { get; internal set; } = int.MaxValue;
  public int LowestAttempt { get; internal set; } = 1;
  public int HighestAttempt { get; internal set; } = int.MaxValue;

  public SuperHexagonScore LowestScore { get; internal set; } = SuperHexagonScore.Zero;
  public SuperHexagonScore HighestScore { get; internal set; } = SuperHexagonScore.MaxValue;

  public string RedeemerUsername { get; internal set; } = null;
  public SuperHexagonLevel[] Levels { get; internal set; } = null;
}