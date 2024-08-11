
using Nixill.Streaming.JoltBot.JSON;
using Nixill.Streaming.JoltBot.Scheduled;

namespace Nixill.Streaming.JoltBot.Files;

public static class PretzelFileWatcher
{
  static FileSystemWatcher Watcher;
  static string LastSent = "";
  static string NextSend = "";
  static bool SendText = false;

  public static async Task SetUp()
  {
    await Task.Delay(0);

    Watcher = new("ext-data\\pretzel\\");
    Watcher.Changed += (sender, e) =>
    {
      if (e.Name == "pretzel.json")
      {
        Task _ = OnFileChanged();
      }
    };

    Watcher.EnableRaisingEvents = true;
  }

  private static async Task OnFileChanged()
  {
    Watcher.EnableRaisingEvents = false;
    await Task.Delay(100);
    PretzelJson.Load();
    if (PretzelJson.IsPlaying)
    {
      NextSend = $"Now playing: {PretzelJson.Title} ({PretzelJson.Credit})";

      if (NextSend != LastSent)
      {
        SendText = true;
      }
    }
    else
    {
      SendText = false;
    }
    Watcher.EnableRaisingEvents = true;
  }

  public static string ShowText()
  {
    if (SendText)
    {
      SendText = false;
      LastSent = NextSend;
      return NextSend;
    }
    else
    {
      return null;
    }
  }
}