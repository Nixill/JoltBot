using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.Discord;
using Nixill.Streaming.JoltBot.JSON;
using Nixill.Streaming.JoltBot.OBS;
using NodaTime;
using NodaTime.Text;
using NodaTime.TimeZones;

namespace Nixill.Streaming.JoltBot.Pipes;

public static class ScreenshotButton
{
  static readonly string ScreenshotFolder = OBSJson.ScreenshotFolder;
  static readonly ZonedDateTimePattern TimePattern = ZonedDateTimePattern.CreateWithInvariantCulture("uuuuMMdd'-'HHmmssfff", null);
  static readonly BclDateTimeZone CurrentZone = BclDateTimeZone.ForSystemDefault();
  static ZonedDateTime Now => SystemClock.Instance.GetCurrentInstant().InZone(CurrentZone);

  public static async Task Press(string format, string source, string sourceType)
  {
    string[] sources = new string[] { };

    if (sourceType == "source")
      sources = new string[] { source };
    else if (sourceType == "special")
      switch (source)
      {
        case "gameSources":
          sources = new string[] { "vcd_GameStick", "gc_Primary" };
          break;
        case "activeScene":
          sources = new string[] { (await OBSRequests.Scenes.GetCurrentProgramScene().Send()).Name };
          break;
        case "previewScene":
          if (await OBSRequests.UI.GetStudioModeEnabled().Send())
            sources = new string[] { (await OBSRequests.Scenes.GetCurrentPreviewScene().Send()).Name };
          else return;
          break;
      }

    foreach (var src in sources)
    {
      Task saveScreenshot = SaveOneScreenshot(format, src);
    }
  }

  public static async Task SaveOneScreenshot(string format, string source)
  {
    try
    {
      var activity = await OBSRequests.Sources.GetSourceActive(source).Send();
      if (!activity.VideoActive && !activity.VideoShowing) return;

      var screenshotPath = $"{ScreenshotFolder}{source}-{TimePattern.Format(Now)}.{format}";
      await OBSRequests.Sources.SaveSourceScreenshot(source, format, screenshotPath).Send();

      await WebhookClient.SendFile("Screenshots", screenshotPath);
    }
    catch (RequestFailedException ex) when (ex.StatusCode == RequestStatus.ResourceNotFound)
    {
      await WebhookClient.SendMessage("Screenshots", $"The source `{source}` does not exist.");
    }
  }
}