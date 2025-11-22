using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.OBS;
using Nixill.Utils;
using Nixill.Utils.Extensions;

namespace Nixill.Streaming.JoltBot.Games.UFO50;

public static partial class BingoSetup
{
  public static async Task ResetBoardAsync()
  {
    await Task.Delay(0);

    // First, we need some information:
    var results = (await new OBSRequestBatch([
      OBSRequests.SceneItems.GetSceneItemId("sc_UFO 50 Bingo", "grp_BingoGoals"),
      OBSRequests.SceneItems.GetSceneItemId("sc_UFO 50 Bingo", "wc_FirefoxForBingo"),
      OBSRequests.SceneItems.GetSceneItemId("sc_UFO 50 Bingo", "grp_BingoGoalsUnrevealed"),
      OBSRequests.SceneItems.GetSceneItemId("grp_BingoGradient", "grd_BingoScores")
    ]).Send()).Results.ToArray();

    int bingoGoalsID = results[0].RequestResult as OBSSingleValueResult<int>;
    int bingoCardID = results[1].RequestResult as OBSSingleValueResult<int>;
    int bingoUnknownsID = results[2].RequestResult as OBSSingleValueResult<int>;

    int bingoGradientBarID = results[3].RequestResult as OBSSingleValueResult<int>;

    // Reset all sixteen game cart images to the blank cartridge
    var gameCartReset = new OBSRequestBatch(Enumerable.Range(1, 2).SelectMany<int, string>(i => [
        $"P{i}CurrentGame",
        .. Enumerable.Range(1, 7).Select(j => $"P{i}Game-{j}")
      ]).Select(s => OBSExtraRequests.Inputs.Image.SetInputImage($"img_Bingo{s}",
        @"C:\Users\Nixill\Documents\Streaming-2024\Images\UFO50\Games\0.png"))).Send();
    BingoGameChanger.Reset();

    // Reset all sixteen game cart frames to the transparent frame
    var gameCartFrameReset = new OBSRequestBatch(Enumerable.Range(1, 2)
      .Product([.. Enumerable.Range(0, 8)], (l, r) => $"img_BingoP{l}Frame-{r}")
      .Select(s => OBSExtraRequests.Inputs.Image.SetInputImage(s,
      @"C:\Users\Nixill\Documents\Streaming-2024\Images\UFO50\Games\blank.png"
    ))).Send();


    // Reset the goal icons to Unknown
    var goalImageReset = new OBSRequestBatch(Enumerable.Range(1, 5)
      .Select(i => OBSExtraRequests.Inputs.Image.SetInputImage($"img_BingoGoal{i}",
        @$"C:\Users\Nixill\Documents\Streaming-2024\Images\UFO50\Goals\Unknown.png"))).Send();
    GoalCursor = 1;

    // Set score and goal numbers to 0
    var goalScoreReset = new OBSRequestBatch(
      Enumerable.Range(1, 2).SelectMany<int, string>(i => [
        $"P{i}Score",
        .. Enumerable.Range(1, 5).Select(j => $"P{i}G{j}Score")
      ]).Select(s => OBSExtraRequests.Inputs.Text.SetText($"txt_Bingo{s}", "0"))).Send();
    BingoScorecard.Reset();

    // Hide goal icons and the bingo card, show the question mark icons
    // instead
    var goalAndCardHiding = new OBSRequestBatch([
      .. Sequence.Of(bingoGoalsID, bingoCardID).Select(id => OBSRequests.SceneItems
        .SetSceneItemEnabled("sc_UFO 50 Bingo", id, false)),

      OBSRequests.SceneItems.SetSceneItemEnabled("sc_UFO 50 Bingo", bingoUnknownsID, true)
    ]).Send();

    // Reset the bingo gradient
    var resetBingoGradient = new OBSRequestBatch([
      OBSExtraRequests.Inputs.Color.SetSize("clr_BingoP1Block", 20, 3),
      OBSExtraRequests.Inputs.Color.SetSize("clr_BingoP2Block", 20, 3),
      OBSRequests.Inputs.SetInputSettings("grd_BingoScores", new JsonObject
      {
        ["height"] = 900
      }),
      OBSRequests.SceneItems.SetSceneItemTransform("grp_BingoGradient", bingoGradientBarID, new SceneItemTransformSetter
      {
        PositionY = 3
      }),
      OBSRequests.SceneItems.SetSceneItemEnabled("grp_BingoGradient", bingoGradientBarID, true)
    ]).Send();

    // Set game card positions/enables back to default
    var gameCardAppearance = new OBSRequestBatch([
      await OBSUtils.SceneItemDisabler("grp_BingoCurrentGamesOpen", "img_BingoP1CurrentGame"),
      await OBSUtils.SceneItemDisabler("grp_BingoCurrentGamesOpen", "img_BingoP2CurrentGame"),
      await OBSUtils.SceneItemDisabler("grp_BingoCurrentGamesReopen", "img_BingoP1CurrentGame"),
      await OBSUtils.SceneItemDisabler("grp_BingoCurrentGamesReopen", "img_BingoP2CurrentGame"),
      await OBSUtils.SceneItemDisabler("grp_BingoCurrentGamesClose", "img_BingoP1CurrentGame"),
      await OBSUtils.SceneItemDisabler("grp_BingoCurrentGamesClose", "img_BingoP2CurrentGame"),
      await OBSUtils.WithSceneItemIndex(OBSRequests.SceneItems.SetSceneItemTransform, "grp_BingoP1HistoryOuter",
        "cln_grp_BingoP1HistoryInner", new SceneItemTransformSetter {
          PositionX = 66
        }),
      await OBSUtils.WithSceneItemIndex(OBSRequests.SceneItems.SetSceneItemTransform, "grp_BingoP2HistoryOuter",
        "cln_grp_BingoP2HistoryInner", new SceneItemTransformSetter {
          PositionX = 66
        })
    ]).Send();

    // Set discord voice states back to hidden
    var discordVoiceOverlays = new OBSRequestBatch([
      await OBSUtils.SceneItemDisabler("sc_UFO 50 Bingo", "brs_UFO 50 Discord 1 200x315"),
      await OBSUtils.SceneItemDisabler("sc_UFO 50 Bingo", "brs_UFO 50 Discord 2 200x315"),
      await OBSUtils.SceneItemDisabler("sc_UFO 50 Bingo", "brs_UFO 50 Discord 3 200x315"),
      await OBSUtils.SceneItemDisabler("sc_UFO 50 Bingo", "brs_UFO 50 Discord 4 200x315"),
      await OBSUtils.SceneItemDisabler("sc_UFO 50 Bingo", "brs_UFO 50 Discord 5 200x315"),
      await OBSUtils.SceneItemDisabler("sc_UFO 50 Bingo", "brs_UFO 50 Discord 6 200x315")
    ]).Send();

    // Turn off all win/loss filters in goal scores
    var goalScoreFilters = new OBSRequestBatch([
      .. Sequence.Of(1, 2)
        .Product([1, 2, 3, 4, 5], (l, r) => $"txt_BingoP{l}G{r}Score")
        .Product(["Win", "Lose"], (l, r) => OBSRequests.Filters.SetSourceFilterEnabled(l, $"cc_{r}", false))
    ]).Send();

    // Set all drafted games to blank
    var blankDraftedGames = new OBSRequestBatch(
      Enumerable.Range(1, 10).Product(["P1", "P2", "Shared"],
        (l, r) => OBSExtraRequests.Inputs.Image.SetInputImage($"img_BingoDraftedGame{r}G{l}",
          @"C:\Users\Nixill\Documents\Streaming-2024\Images\UFO50\Games\blank.png"))).Send();

    // Set all games on the draft board to unselected
    var deselectDraftBoard = new OBSRequestBatch(
      Enumerable.Range(1, 50)
        .Select(i => OBSExtraRequests.Inputs.Image.SetInputImage($"img_UFO50GameCard{i}",
          @$"C:\Users\Nixill\Documents\Streaming-2024\Images\UFO50\Games\d{i}.png"))).Send();
    await BingoDraftController.Reset();

    // Switch to the general goals (disable the drafted games view)
    var generalsOverDraft = new OBSRequestBatch(
      await OBSUtils.SceneItemDisabler("sc_UFO 50 Bingo", "grp_BingoDraftedGames"),
      await OBSUtils.SceneItemEnabler("sc_UFO 50 Bingo", "grp_BingoP1GoalScores"),
      await OBSUtils.SceneItemEnabler("sc_UFO 50 Bingo", "grp_BingoP2GoalScores")
    // Hidden goal list is covered above
    ).Send();

    var enableStudioMode = OBSRequests.UI.SetStudioModeEnabled(true).Send();

    await SceneSwitcher.SwitchTo("sc_UFO 50 Setup", []);
    await enableStudioMode;

    // Set preview scene to behind-the-scenes
    var changeScene1 = new OBSRequestBatch([
      OBSRequests.Scenes.SetCurrentPreviewScene("sc_UFO 50 Bingo BTS")
    ]).Send();

    Dictionary<string, OBSRequestBatchResult> responses = new Dictionary<string, OBSRequestBatchResult>
    {
      [nameof(gameCartReset)] = await gameCartReset,
      [nameof(gameCartFrameReset)] = await gameCartFrameReset,
      [nameof(goalImageReset)] = await goalImageReset,
      [nameof(goalScoreReset)] = await goalScoreReset,
      [nameof(goalAndCardHiding)] = await goalAndCardHiding,
      [nameof(resetBingoGradient)] = await resetBingoGradient,
      [nameof(changeScene1)] = await changeScene1,
      [nameof(gameCardAppearance)] = await gameCardAppearance,
      [nameof(discordVoiceOverlays)] = await discordVoiceOverlays,
      [nameof(goalScoreFilters)] = await goalScoreFilters,
      [nameof(blankDraftedGames)] = await blankDraftedGames,
      [nameof(deselectDraftBoard)] = await deselectDraftBoard,
      [nameof(generalsOverDraft)] = await generalsOverDraft
    };

    if (responses.All(r => r.Value.FinishedWithoutErrors)) Logger.LogInformation("Successfully reset bingo scene!");
    else
    {
      if (!responses[nameof(gameCartReset)].FinishedWithoutErrors) Logger.LogError("Error resetting game cart icons.");
      if (!responses[nameof(gameCartFrameReset)].FinishedWithoutErrors) Logger.LogError("Error resetting game cart frames.");
      if (!responses[nameof(goalImageReset)].FinishedWithoutErrors) Logger.LogError("Error resetting goal images");
      if (!responses[nameof(goalScoreReset)].FinishedWithoutErrors) Logger.LogError("Error resetting goal scores");
      if (!responses[nameof(goalAndCardHiding)].FinishedWithoutErrors) Logger.LogError("Error hiding goals and cards");
      if (!responses[nameof(resetBingoGradient)].FinishedWithoutErrors) Logger.LogError("Error resetting bingo gradient bar");
      if (!responses[nameof(changeScene1)].FinishedWithoutErrors) Logger.LogError("Error setting proper scenes");
      if (!responses[nameof(gameCardAppearance)].FinishedWithoutErrors) Logger.LogError("Error resetting game cart positions/enables.");
      if (!responses[nameof(discordVoiceOverlays)].FinishedWithoutErrors) Logger.LogError("Error hiding Discord voice overlays");
      if (!responses[nameof(goalScoreFilters)].FinishedWithoutErrors) Logger.LogError("Error removing win/loss scoring filters");
      if (!responses[nameof(blankDraftedGames)].FinishedWithoutErrors) Logger.LogError("Error clearing the draft selection");
      if (!responses[nameof(deselectDraftBoard)].FinishedWithoutErrors) Logger.LogError("Error deselecting the draft board");
      if (!responses[nameof(generalsOverDraft)].FinishedWithoutErrors) Logger.LogError("Error setting generals over draft");
    }

    await BingoMusicController.PlayIntroMusic();
  }

}