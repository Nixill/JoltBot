using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.OBS;

namespace Nixill.Streaming.JoltBot.Pipes;

public static class ScreenshotButton
{
  public static async Task Press(string format, string source, string sourceType)
  {
    string[] sources;

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
  }
}