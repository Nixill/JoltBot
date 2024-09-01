using System.Data;
using System.Data.Common;
using CommunityToolkit.HighPerformance.Helpers;
using Nixill.Collections.Grid.CSV;
using Nixill.Streaming.JoltBot.Twitch.Events.Rewards;
using NodaTime;
using NodaTime.Text;

namespace Nixill.Streaming.JoltBot.Data;

public static class SuperHexagonCSVs
{
  static readonly CSVObjectCollection<SuperHexagonAttempt> Attempts
    = CSVObjectCollection.ParseObjectsFromFile("data/SuperHexagon/attempts.csv", dict => new SuperHexagonAttempt
    {
      AttemptId = int.Parse(dict["attempt_id"]),
      RedemptionId = int.Parse(dict["redemption_id"]),
      Score = SuperHexagonScore.Parse(dict["score"]),
      Highlight = dict.TryGetValue("highlight", out string highlight) ? new Uri(highlight) : null,
      Notes = dict["notes"]
    });

  static readonly CSVObjectCollection<SuperHexagonRedemption> Redemptions
    = CSVObjectCollection.ParseObjectsFromFile("data/SuperHexagon/redemptions.csv", dict => new SuperHexagonRedemption
    {
      RedemptionId = int.Parse(dict["redemption_id"]),
      Date = LocalDatePattern.Iso.Parse(dict["date"]).Value,
      RedeemerUsername = dict["redeemer"],
      RedeemerID = dict["redeemer_id"],
      Level = Enum.Parse<SuperHexagonLevel>(dict["level"])
    });

  public static int GetLastAttemptId() => Attempts.Last().AttemptId;
  public static int GetLastRedemptionId() => Redemptions.Last().RedemptionId;
}

public readonly struct SuperHexagonAttempt
{
  public required int AttemptId { get; init; }
  public required int RedemptionId { get; init; }
  public required SuperHexagonScore Score { get; init; }
  public Uri Highlight { get; init; }
  public string Notes { get; init; }
}

public readonly struct SuperHexagonRedemption
{
  public required int RedemptionId { get; init; }
  public required LocalDate Date { get; init; }
  public required string RedeemerUsername { get; init; }
  public string RedeemerID { get; init; }
  public required SuperHexagonLevel Level { get; init; }
}