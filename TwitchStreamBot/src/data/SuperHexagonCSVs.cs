using Nixill.Collections;
using Nixill.Streaming.JoltBot.Twitch.Events.Rewards;
using NodaTime;
using NodaTime.Text;

namespace Nixill.Streaming.JoltBot.Data;

public static class SuperHexagonCSVs
{
  internal static readonly CSVObjectCollection<SuperHexagonAttempt> Attempts
    = CSVObjectCollection.ParseObjectsFromFile("data/SuperHexagon/attempts.csv", dict => new SuperHexagonAttempt
    {
      AttemptId = int.Parse(dict["attempt_id"]),
      RedemptionID = int.Parse(dict["redemption_id"]),
      Score = SuperHexagonScore.Parse(dict["score"]),
      Highlight = dict.TryGetValue("highlight", out string highlight) ? new Uri(highlight) : null,
      Notes = dict["notes"]
    });

  internal static IDictionary<string, string> FormatAttempt(SuperHexagonAttempt input)
    => new Dictionary<string, string>
    {
      ["attempt_id"] = input.AttemptId.ToString(),
      ["redemption_id"] = input.RedemptionID.ToString(),
      ["score"] = input.Score.ToString(),
      ["highlight"] = input.Highlight?.ToString() ?? "",
      ["notes"] = input.Notes?.ToString() ?? ""
    };

  internal static readonly CSVObjectCollection<SuperHexagonRedemption> Redemptions
    = CSVObjectCollection.ParseObjectsFromFile("data/SuperHexagon/redemptions.csv", dict => new SuperHexagonRedemption
    {
      RedemptionID = int.Parse(dict["redemption_id"]),
      Date = LocalDatePattern.Iso.Parse(dict["date"]).Value,
      RedeemerUsername = dict["redeemer"],
      RedeemerID = dict["redeemer_id"],
      Level = Enum.Parse<SuperHexagonLevel>(dict["level"])
    });

  internal static IDictionary<string, string> FormatRedemption(SuperHexagonRedemption input)
    => new Dictionary<string, string>
    {
      ["redemption_id"] = input.RedemptionID.ToString(),
      ["date"] = LocalDatePattern.Iso.Format(input.Date),
      ["redeemer"] = input.RedeemerUsername,
      ["redeemer_id"] = input.RedeemerID ?? "",
      ["level"] = input.Level.ToString()
    };

  internal static async Task AddRedemption(SuperHexagonRedemption input)
  {
    await File.AppendAllTextAsync("data/SuperHexagon/redemptions.csv", "\n" + Redemptions.NewRow(input, FormatRedemption));
  }

  internal static async Task AddAttempt(SuperHexagonAttempt input)
  {
    await File.AppendAllTextAsync("data/SuperHexagon/attempts.csv", "\n" + Attempts.NewRow(input, FormatAttempt));
  }

  internal static SuperHexagonRedemption GetRedemption(this SuperHexagonAttempt attempt) => Redemptions[attempt.RedemptionID];
  internal static IEnumerable<SuperHexagonAttempt> GetAttempts(this SuperHexagonRedemption redemption) => Attempts.Where(a => a.RedemptionID == redemption.RedemptionID);
  internal static IEnumerable<(SuperHexagonAttempt Attempt, SuperHexagonRedemption Redemption)> Joined => Attempts.Join(Redemptions, a => a.RedemptionID, r => r.RedemptionID, (a, r) => (a, r));
}

public readonly struct SuperHexagonAttempt
{
  public required int AttemptId { get; init; }
  public required int RedemptionID { get; init; }
  public required SuperHexagonScore Score { get; init; }
  public Uri Highlight { get; init; }
  public string Notes { get; init; }
}

public readonly struct SuperHexagonRedemption
{
  public required int RedemptionID { get; init; }
  public required LocalDate Date { get; init; }
  public required string RedeemerUsername { get; init; }
  public string RedeemerID { get; init; }
  public required SuperHexagonLevel Level { get; init; }
}