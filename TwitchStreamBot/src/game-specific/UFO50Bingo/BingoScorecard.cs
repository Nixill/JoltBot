using System.Text.Json.Nodes;
using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.OBS;

namespace Nixill.Streaming.JoltBot.Games.UFO50;

public static class BingoScorecard
{
  static int[][] Scores = [[/* this array intentionally left blank */],
    [0, 0, 0, 0, 0, 0],
    [0, 0, 0, 0, 0, 0]];

  static int? _gradientSceneItemID = null;

  static async Task<int> GetGradientSceneItemIDAsync()
    => _gradientSceneItemID ??= await OBSRequests.SceneItems.GetSceneItemId("grp_BingoGradient", "grd_BingoScores").Send();

  public static async Task IncrementGoalScore(int player, int goal, string direction)
  {
    int score;
    if (direction == "+") score = ++Scores[player][goal];
    else score = Scores[player][goal] = int.Max(0, Scores[player][goal] - 1);

    await OBSExtraRequests.Inputs.Text.SetText($"txt_BingoP{player}G{goal}Score", score.ToString()).Send();
  }

  public static async Task IncrementBigScore(int player, string direction)
  {
    bool scoresWereTied = Scores[1][0] == Scores[2][0] && Scores[1][0] > 0;

    int score;
    if (direction == "+") score = ++Scores[player][0];
    else score = Scores[player][0] = int.Max(0, Scores[player][0] - 1);

    bool scoresNowTied = Scores[1][0] == Scores[2][0] && Scores[1][0] > 0;

    _ = OBSExtraRequests.Inputs.Text.SetText($"txt_BingoP{player}Score", score.ToString()).Send();

    if (scoresWereTied) _ = OBSExtraRequests.Inputs.Text.SetText($"txt_BingoP{3 - player}Score",
      Scores[3 - player][0].ToString()).Send();

    if (scoresNowTied)
    {
      if (direction == "+") _ = OBSExtraRequests.Inputs.Text.SetText($"txt_BingoP{3 - player}Score",
        $"{Scores[3 - player][0]}.").Send();
      else _ = OBSExtraRequests.Inputs.Text.SetText($"txt_BingoP{player}Score", $"{score}.").Send();
    }

    // Also resize the gradient bar!
    if (Scores[1][0] + Scores[2][0] <= 25)
    {
      int playerBlockHeight = 3 + 36 * score;
      int gradientBlockHeight = 36 * (25 - (Scores[1][0] + Scores[2][0]));
      int gradientPosition = 3 + 36 * Scores[1][0];

      _ = new OBSRequestBatch([
        OBSExtraRequests.Inputs.Color.SetSize($"clr_BingoP{player}Block", 20, playerBlockHeight),
        OBSRequests.Inputs.SetInputSettings("grd_BingoScores", new JsonObject {
          ["height"] = gradientBlockHeight
        }),
        OBSRequests.SceneItems.SetSceneItemTransform("grp_BingoGradient", await GetGradientSceneItemIDAsync(),
          new SceneItemTransformSetter {
            PositionY = gradientPosition
          }),
        OBSRequests.SceneItems.SetSceneItemEnabled("grp_BingoGradient", await GetGradientSceneItemIDAsync(),
          Scores[1][0] + Scores[2][0] < 25)
      ]).Send();
    }
  }

  public static async Task ToggleGeneralWin(string player, string goal)
  {
    bool filterAlreadyApplied =
      (await OBSRequests.Filters.GetSourceFilter($"txt_BingoP{player}G{goal}Score", "cc_Win").Send())
        .Enabled;

    await new OBSRequestBatch([
      OBSRequests.Filters.SetSourceFilterEnabled($"txt_BingoP{player}G{goal}Score", "cc_Win", !filterAlreadyApplied),
      OBSRequests.Filters.SetSourceFilterEnabled($"txt_BingoP{(player == "1" ? "2" : "1")}G{goal}Score", "cc_Lose", !filterAlreadyApplied)
    ]).Send();
  }

  public static void Reset()
    => Scores = [[/* this array intentionally left blank */],
      [0, 0, 0, 0, 0, 0],
      [0, 0, 0, 0, 0, 0]];
}