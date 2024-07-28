using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.Twitch;
using Nixill.Streaming.JoltBot.Twitch.Commands;

namespace Nixill.Streaming.JoltBot.OBS;

public static class JoltOBSEventHandlers
{
  public static async Task StreamStarted(object sender, OutputStateChanged e)
  {
    await ModCommands.OpenChatCommand(null);
    AdManager.TryStartAd();
    StreamStopper.EndingForStreamTitle = false;
    await StreamStopper.HandleStreamStart();
  }

  public static async Task StreamStopped(Object sender, OutputStateChanged e)
  {
    await Task.Delay(0);
    if (!StreamStopper.EndingForStreamTitle)
    {
      Task _ = CloseChatAfterTenSeconds();
    }
  }

  public static async Task CloseChatAfterTenSeconds()
  {
    await JoltChatBot.Chat("Stream is over and chat will be locked in ten seconds.");
    await Task.Delay(TimeSpan.FromSeconds(10));
    await ModCommands.CloseChatCommand(null);
  }
}