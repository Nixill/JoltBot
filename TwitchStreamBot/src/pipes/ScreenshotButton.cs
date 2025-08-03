using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.Discord;
using Nixill.Streaming.JoltBot.Data;
using Nixill.Streaming.JoltBot.OBS;
using NodaTime;
using NodaTime.Text;
using NodaTime.TimeZones;
using Nixill.Utils.Extensions;

namespace Nixill.Streaming.JoltBot.Pipes;

public static class ScreenshotButton
{
  static readonly string ScreenshotFolder = OBSJson.ScreenshotFolder;
  static readonly ZonedDateTimePattern TimePattern = ZonedDateTimePattern.CreateWithInvariantCulture("uuuuMMdd'-'HHmmssfff", null);
  static readonly BclDateTimeZone CurrentZone = BclDateTimeZone.ForSystemDefault();
  static ZonedDateTime Now => SystemClock.Instance.GetCurrentInstant().InZone(CurrentZone);

  public static async Task Parse(List<string> pars)
  {
    string format = "png";
    string sourceType = "namedSources";
    List<string> sources = [];

    while (pars.TryPop(out string val))
    {
      if ((val == "-f" || val == "--format") && pars.TryPop(out string fmt)) format = fmt;
      else if (val == "-j" || val == "--jpg" || val == "--jpeg") format = "jpg";
      else if (val == "--gameSources") sourceType = "gameSources";
      else if (val == "--activeScene") sourceType = "activeScene";
      else if (val == "--previewScene") sourceType = "previewScene";
      else if (val == "--") break;
      else sources.Add(val);
    }

    sources.AddRange(pars);

    await Press(format, sources, sourceType);
  }

  public static async Task Press(string format, List<string> sources, string sourceType)
  {
    switch (sourceType)
    {
      case "gameSources":
        sources = ["vcd_GameStick", "gc_Primary", "dc_TopScreen", "gc_MelonDS"];
        break;
      case "activeScene":
        sources = [(await OBSRequests.Scenes.GetCurrentProgramScene().Send()).Name];
        break;
      case "previewScene":
        if (await OBSRequests.UI.GetStudioModeEnabled().Send())
          sources = [(await OBSRequests.Scenes.GetCurrentPreviewScene().Send()).Name];
        else return;
        break;
        // can also be "namedSources", in which case don't modify the
        // existing sources list
    }

    foreach (var src in sources)
    {
      Task _ = SaveOneScreenshot(format, src);
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