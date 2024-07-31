using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Nixill.Streaming.JoltBot.OBS;
using Nixill.Streaming.JoltBot.Twitch;
using NodaTime;
using NodaTime.Text;

namespace Nixill.Streaming.JoltBot.Pipes;

public static class PipeRunner
{
  static ILogger Logger = Log.Factory.CreateLogger(typeof(PipeRunner));

  public static void SetUp()
  {
    Task _ = Task.Run(PipeServer.ListenForMessages);

    PipeServer.MessageReceived += (sender, args) =>
    {
      JsonObject data = args.Data;
      Logger.LogInformation($"Received message from pipe: {data.ToJsonString()}");

      string req = (string)data["request"];

      try
      {
        Task _;
        switch (req)
        {
          case null: break;
          case "Ad.Start":
            AdManager.TryStartAd((int?)data["length"] ?? 180);
            break;
          case "Ad.Stop":
            AdManager.TryStopAd();
            break;
          case "Commands.Run":
            _ = Task.Run(() => CommandDispatch.Dispatch(((string)data["commandText"]).Split(" ").ToList()));
            break;
          case "Markers.Place":
            _ = Task.Run(() => MarkerButton.Place());
            break;
          case "Scenes.Switch":
            string scene = (string)data["scene"];
            string[] show = JsonSerializer.Deserialize<string[]>(data["show"]);
            _ = Task.Run(() => SceneSwitcher.SwitchTo(scene, show));
            break;
          case "Screenshots.Save":
            string format = (string)data["format"] ?? "png";
            string source, sourceType;
            if (data["source"] != null)
            {
              source = (string)data["source"];
              sourceType = "source";
            }
            else
            {
              source = (string)data["special"] ?? "gameSources";
              sourceType = "special";
            }
            _ = Task.Run(() => ScreenshotButton.Press(format, source, sourceType));
            break;
          case "Upcoming.Read":
            LocalDate? date = null;
            if (data.ContainsKey("date")) date = LocalDatePattern.Iso.Parse((string)data["date"]).Value;
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
}