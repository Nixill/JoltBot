using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Nixill.Streaming.JoltBot.Games.UFO50;
using Nixill.Streaming.JoltBot.OBS;
using Nixill.Streaming.JoltBot.Twitch;
using Nixill.Streaming.JoltBot.Twitch.Events;
using Nixill.Utils;
using Nixill.Utils.Extensions;
using NodaTime;
using NodaTime.Text;

namespace Nixill.Streaming.JoltBot.Pipes;

public static partial class PipeRunner
{
  static ILogger Logger = Log.Factory.CreateLogger(typeof(PipeRunner));

  public static void SetUp()
  {
    Task _ = Task.Run(PipeServer.ListenForMessages);

    PipeServer.MessageReceived += (sender, args) =>
    {
      List<string> pars = args.Data;
      Logger.LogInformation($"Received message from pipe: {pars.StringJoin(" ")}");

      try
      {
        string req = pars.Pop();

        switch (req)
        {
          case null or "": break;
          case "Ad.Start":
            AdManager.TryStartAd(NullParser.Int(pars.Pop(), 180));
            break;
          case "Ad.Stop":
            AdManager.TryStopAd();
            break;
          case "Commands.Run":
            _ = Task.Run(() => CommandDispatch.Dispatch(pars));
            break;
          case "Markers.Place":
            _ = Task.Run(MarkerButton.Place);
            break;
          case "Rewards.Refresh":
            _ = JoltRewardDispatch.Modify();
            break;
          case "Scenes.Switch":
            string scene = ParseName(pars);
            string[] show = [.. ParseNames(pars)];
            _ = Task.Run(() => SceneSwitcher.SwitchTo(scene, show));
            break;
          case "Screenshots.Save":
            _ = Task.Run(() => ScreenshotButton.Parse(pars));
            break;
          case "Upcoming.Read":
            LocalDate? date = null;
            if (pars.Count > 0) date = LocalDatePattern.Iso.Parse(pars.Pop()).Value;
            _ = Task.Run(() => EndScreenManager.UpdateStreamData(date));
            break;
          case "Upcoming.Write":
            _ = Task.Run(EndScreenManager.UpdateStreamScene);
            break;
          case "UFO50.Reset":
            _ = Task.Run(BingoSetup.ResetBoardAsync);
            break;
          case "UFO50.ChangeGoal":
            if (pars.Count > 1) pars.Pop();
            _ = Task.Run(() => BingoSetup.ChangeGoalImage(pars.Pop()));
            break;
          case "UFO50.PlayerColor":
            _ = Task.Run(() => BingoSetup.SetPlayerColor(pars.Pop(), pars.Pop()));
            break;
          case "UFO50.FinishSetup":
            _ = Task.Run(BingoSetup.FinishSetup);
            break;
          case "UFO50.Discord":
            _ = Task.Run(BingoSetup.SetDiscordCapture);
            break;
          case "UFO50.Reveal":
            _ = Task.Run(BingoSetup.RevealCardAndGoals);
            break;
          case "UFO50.GameSelect":
            int player = int.Parse(pars.Pop());
            BingoGameChanger.LastPlayerChanged = player;
            break;
          case "UFO50.OpenGame":
            _ = Task.Run(() => BingoGameChanger.SwitchToGame(int.Parse(pars.Pop()), pars.Count > 0));
            break;
          case "UFO50.Score":
            if (pars.Count >= 3) _ = Task.Run(() => BingoScorecard.IncrementGoalScore(int.Parse(pars.Pop()),
              int.Parse(pars.Pop()), pars.Pop()));
            else _ = Task.Run(() => BingoScorecard.IncrementBigScore(int.Parse(pars.Pop()), pars.Pop()));
            break;
          case "UFO50.GeneralWin":
            _ = Task.Run(() => BingoScorecard.ToggleGeneralWin(pars.Pop(), pars.Pop()));
            break;
          case "UFO50.UpgradeCart":
            _ = Task.Run(() => BingoGameChanger.UpgradeCartridge(int.Parse(pars.Pop())));
            break;
        }
      }
      catch (Exception ex)
      {
        Logger.LogError(ex, "Exception while running piped command");
      }
    };
  }

  public static string ParseName(List<string> pars)
  {
    string output = "";
    while (pars.TryPop(out string val))
    {
      Match mtc = TrailingBackslashes().Match(val);
      output += mtc.Groups[1].Value;
      output += new string('\\', mtc.Groups[2].Length / 2);
      if (mtc.Groups[2].Length % 2 == 1) output += " ";
      else return output;
    }

    return output;
  }

  public static IEnumerable<string> ParseNames(List<string> pars)
  {
    string output = "";

    while (pars.TryPop(out string val))
    {
      Match mtc = TrailingBackslashes().Match(val);
      output += mtc.Groups[1].Value;
      output += new string('\\', mtc.Groups[2].Length / 2);
      if (mtc.Groups[2].Length % 2 == 1) output += " ";
      else
      {
        yield return output;
        output = "";
      }
    }

    if (output != "") yield return output;
  }

  [GeneratedRegex(@"(.*?)(\\*)$")]
  private static partial Regex TrailingBackslashes();
}