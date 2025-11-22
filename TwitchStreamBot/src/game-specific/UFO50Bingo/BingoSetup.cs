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

  static async Task SetBingoRulesEnabled(string whichRules, string requestedRules)
  {
    await (await OBSUtils.SceneItemSetter("sc_UFO 50 Rules", $"img_{whichRules}RulesBody", whichRules == requestedRules)).Send();
    await (await OBSUtils.SceneItemSetter("grp_BingoUIColors3", $"img_{whichRules}RulesHeader", whichRules == requestedRules)).Send();
  }

  public static async Task FinishSetup(string requestedRules)
  {
    await Task.Delay(0);
    _ = BingoMusicController.PlayForever();
    _ = OBSRequests.UI.SetStudioModeEnabled(false).Send();
    _ = SetBingoRulesEnabled("Standard", requestedRules);
    _ = SetBingoRulesEnabled("Playoff", requestedRules);
    _ = SetBingoRulesEnabled("PlayoffBo3", requestedRules);
    _ = SetBingoRulesEnabled("Underground", requestedRules);
    _ = OBSRequests.Scenes.SetCurrentProgramScene("sc_UFO 50 Rules").Send();
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