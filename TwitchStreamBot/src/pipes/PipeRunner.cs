using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
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

        Task _;
        switch (req)
        {
          case null or "": break;
          case "Ad.Start":
            AdManager.TryStartAd(NumParser.Int(pars.Pop(), 180));
            break;
          case "Ad.Stop":
            AdManager.TryStopAd();
            break;
          case "Commands.Run":
            _ = Task.Run(() => CommandDispatch.Dispatch(pars));
            break;
          case "Markers.Place":
            _ = Task.Run(() => MarkerButton.Place());
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
            _ = Task.Run(() => EndScreenManager.UpdateStreamScene());
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