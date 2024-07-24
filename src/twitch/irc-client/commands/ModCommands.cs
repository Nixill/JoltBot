using Nixill.Streaming.JoltBot.JSON;
using Nixill.Streaming.JoltBot.Twitch.Api;
using TwitchLib.Api.Helix.Models.Chat.ChatSettings;
using Args = TwitchLib.Client.Events.OnChatCommandReceivedArgs;

namespace Nixill.Streaming.JoltBot.Twitch.Commands;

[CommandContainer]
public static class ModCommands
{
  [Command("closechat")]
  public static async Task CloseChatCommand(Args ev)
  {
    await JoltApiClient.WithToken((api, id) => api.Helix.Chat.UpdateChatSettingsAsync(
      id,
      id,
      new ChatSettings
      {
        EmoteMode = true,
        FollowerMode = true,
        FollowerModeDuration = 129600,
        SlowMode = true,
        SlowModeWaitTime = 120,
        SubscriberMode = true
      }
    ));
  }

  [Command("openchat")]
  public static async Task OpenChatCommand(Args ev)
  {
    await JoltApiClient.WithToken((api, id) => api.Helix.Chat.UpdateChatSettingsAsync(
      id,
      id,
      new ChatSettings
      {
        EmoteMode = false,
        FollowerMode = false,
        SlowMode = false,
        SubscriberMode = false
      }
    ));
  }
}
