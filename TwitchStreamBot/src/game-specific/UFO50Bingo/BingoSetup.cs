using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Nixill.OBSWS;
using Nixill.OBSWS.Extensions;
using Nixill.OBSWS.Utils;
using Nixill.Streaming.JoltBot.Data.UFO50;
using Nixill.Streaming.JoltBot.OBS;
using Nixill.Streaming.JoltBot.Pipes;
using Nixill.Streaming.JoltBot.Twitch.Api;
using Nixill.Utils;
using Nixill.Utils.Extensions;

namespace Nixill.Streaming.JoltBot.Games.UFO50;

public static partial class BingoSetup
{
  static readonly ILogger Logger = Log.Factory.CreateLogger(typeof(BingoSetup));

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

    var enableStudioMode = OBSRequests.UI.SetStudioModeEnabled(true).Send();

    await SceneSwitcher.SwitchTo("sc_UFO 50 Setup", []);
    await enableStudioMode;

    // Set preview scene to behind-the-scenes
    var changeScene1 = new OBSRequestBatch([
      OBSRequests.Scenes.SetCurrentPreviewScene("sc_UFO 50 Bingo BTS")
    ]).Send();

    OBSRequestBatchResult[] responses = [
      await gameCartReset,
      await gameCartFrameReset,
      await goalImageReset,
      await goalScoreReset,
      await goalAndCardHiding,
      await resetBingoGradient,
      await changeScene1,
      await gameCardAppearance,
      await discordVoiceOverlays,
      await goalScoreFilters
    ];

    if (responses.All(r => r.FinishedWithoutErrors)) Logger.LogInformation("Successfully reset bingo scene!");
    else
    {
      if (!responses[0].FinishedWithoutErrors) Logger.LogError("Error resetting game cart icons.");
      if (!responses[1].FinishedWithoutErrors) Logger.LogError("Error resetting game cart frames.");
      if (!responses[2].FinishedWithoutErrors) Logger.LogError("Error resetting goal images");
      if (!responses[3].FinishedWithoutErrors) Logger.LogError("Error resetting goal scores");
      if (!responses[4].FinishedWithoutErrors) Logger.LogError("Error hiding goals and cards");
      if (!responses[5].FinishedWithoutErrors) Logger.LogError("Error resetting bingo gradient bar");
      if (!responses[6].FinishedWithoutErrors) Logger.LogError("Error setting proper scenes");
      if (!responses[7].FinishedWithoutErrors) Logger.LogError("Error resetting game cart positions/enables.");
      if (!responses[8].FinishedWithoutErrors) Logger.LogError("Error hiding Discord voice overlays");
      if (!responses[9].FinishedWithoutErrors) Logger.LogError("Error removing win/loss scoring filters");
    }

    await BingoMusicController.PlayIntroMusic();
  }

  static int GoalCursor = 1;

  public static async Task ChangeGoalImage(string toImage)
  {
    await OBSExtraRequests.Inputs.Image.SetInputImage($"img_BingoGoal{GoalCursor}",
      @$"C:\Users\Nixill\Documents\Streaming-2024\Images\UFO50\Goals\{toImage}.png").Send();
    GoalCursor++;
    if (GoalCursor == 6) GoalCursor = 1;
  }

  public static async Task SetPlayerColor(string player, string toColor)
  {
    string s = player;
    UFO50Color color = UFO50BingosyncCsv.GetColor(toColor);

    await new OBSRequestBatch([
      OBSExtraRequests.Inputs.Color.SetColor($"clr_BingoP{s}Block", ColorConversions.FromRGB(color.MainColor)),
      OBSExtraRequests.Filters.ColorCorrection.SetMultipliedColor($"grp_BingoP{s}Colors", "cc_BingoPlayer",
        ColorConversions.FromRGB(color.MainColor)),
      OBSExtraRequests.Filters.ColorCorrection.SetMultipliedColor($"grp_BingoP{s}Preview", "cc_BingoPlayer",
        ColorConversions.FromRGB(color.MainColor)),
      .. Sequence.Of("", "G1", "G2", "G3", "G4", "G5").Select(g =>
        OBSExtraRequests.Inputs.Text.SetOutlineColor($"txt_BingoP{s}{g}Score", ColorConversions.FromRGB(color.DarkColor))
      ),
      OBSRequests.Inputs.SetInputSettings("grd_BingoScores", new JsonObject {
        [s == "1" ? "from_color" : "to_color_1"] = ColorConversions.FromRGB(color.MainColor)
      })
    ]).Send();
  }

  public static async Task SetDiscordCapture()
  {
    IEnumerable<PropertyItem> windows = await OBSRequests.Inputs.GetInputPropertiesListPropertyItems("wc_DiscordForBingo", "window").Send();

    try
    {
      var discordWindow = windows.First(pi => pi.Enabled && ((string)pi.Value).EndsWith(":Chrome_WidgetWin_1:Discord.exe")
        && !((string)pi.Value).EndsWith(" - Discord:Chrome_WidgetWin_1:Discord.exe"));

      List<OBSRequest> requests = [OBSRequests.Inputs.SetInputSetting("wc_DiscordForBingo", "window", (string)discordWindow.Value)];

      var match = PrivateMatchRegex.Match((string)discordWindow.Value);
      if (match.Success)
      {
        requests.Add(
          await OBSUtils.SceneItemEnabler("sc_UFO 50 Bingo", $"brs_UFO 50 Discord {match.Groups[1].Value} 200x315")
        );
      }

      await new OBSRequestBatch(requests).Send();
    }
    catch (InvalidOperationException)
    { /* do nothing */ }
  }

  [GeneratedRegex(@"^PrivateMatch(\d):Chrome_WidgetWin_1:Discord.exe$")]
  static partial Regex PrivateMatchRegex { get; }

  public static async Task FinishSetup()
  {
    await BingoMusicController.PlayForever();
    await OBSRequests.UI.SetStudioModeEnabled(false).Send();
    await OBSRequests.Scenes.SetCurrentProgramScene("sc_UFO 50 Rules").Send();
  }

  public static Task CloseRules()
    => OBSRequests.Scenes.SetCurrentProgramScene("sc_UFO 50 Bingo").Send();

  public static async Task RevealCardAndGoals()
  {
    _ = new OBSRequestBatch([
      await OBSUtils.SceneItemDisabler("sc_UFO 50 Bingo", "grp_BingoGoalsUnrevealed"),
      await OBSUtils.SceneItemEnabler("sc_UFO 50 Bingo", "grp_BingoGoals"),
      await OBSUtils.SceneItemEnabler("sc_UFO 50 Bingo", "wc_FirefoxForBingo")
    ]).Send();
    _ = MarkerButton.Place("Bingo card revealed!");
  }
}