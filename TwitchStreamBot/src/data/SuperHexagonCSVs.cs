using System.Data;
using System.Data.Common;
using Nixill.Collections.Grid.CSV;
using Nixill.Streaming.JoltBot.Twitch.Events.Rewards;
using NodaTime;

namespace Nixill.Streaming.JoltBot.Data;

public static class SuperHexagonCSVs
{
  static readonly DataTable AttemptsTable = DataTableCSVParser.FileToDataTable("data/SuperHexagon/attempts.csv",
      [
        new DataColumn("attempt_num", typeof(int)),
        new DataColumn("redemption_num", typeof(int)) { AllowDBNull = false },
        new DataColumn("score", typeof(SuperHexagonScore)) { AllowDBNull = false },
        new DataColumn("highlight", typeof(Uri)),
        new DataColumn("notes", typeof(string))
      ], primaryKey: ["attempt_num"]
    );
  static IEnumerable<SuperHexagonAttempt> Attempts => AttemptsTable.AsEnumerable()
    .Select(r => new SuperHexagonAttempt
    {
      AttemptNum = (int)r["attempt_num"],
      RedemptionNum = (int)r["redemption_num"],
      Score = (SuperHexagonScore)r["score"],
      Highlight = (Uri)r["highlight"],
      Notes = (string)r["notes"]
    });

  static readonly DataTable RedemptionsTable = DataTableCSVParser.FileToDataTable("data/SuperHexagon/redemptions.csv",
      [
        new DataColumn("redemption_num", typeof(int)),
        new DataColumn("date", typeof(LocalDate)) { AllowDBNull = false },
        new DataColumn("redeemer", typeof(string)) { AllowDBNull = false },
        new DataColumn("redeemer_id", typeof(string)),
        new DataColumn("level", typeof(SuperHexagonLevel)) { AllowDBNull = false }
      ], primaryKey: ["redemption_num"]
    );
  static IEnumerable<SuperHexagonRedemption> Redemptions => RedemptionsTable.AsEnumerable()
    .Select(r => new SuperHexagonRedemption
    {
      RedemptionNum = (int)r["redemption_num"],
      Date = (LocalDate)r["date"],
      RedeemerUsername = (string)r["redeemer"],
      RedeemerID = (string)r["redeemer_id"],
      Level = (SuperHexagonLevel)r["level"]
    });

  public static int GetLastAttemptNum() => Attempts.Last().AttemptNum;
  public static int GetLastRedemptionNum() => Redemptions.Last().RedemptionNum;
}

public readonly struct SuperHexagonAttempt
{
  public required int AttemptNum { get; init; }
  public required int RedemptionNum { get; init; }
  public required SuperHexagonScore Score { get; init; }
  public Uri Highlight { get; init; }
  public string Notes { get; init; }
}

public readonly struct SuperHexagonRedemption
{
  public required int RedemptionNum { get; init; }
  public required LocalDate Date { get; init; }
  public required string RedeemerUsername { get; init; }
  public string RedeemerID { get; init; }
  public required SuperHexagonLevel Level { get; init; }
}