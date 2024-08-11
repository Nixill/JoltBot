using Nixill.Streaming.JoltBot.Twitch.Api;
using TwitchLib.Api.Helix.Models.Chat.ChatSettings;

namespace Nixill.Streaming.JoltBot.Twitch.Commands;

[CommandContainer]
public static class ModCommands
{
  [Command("closechat")]
  [AllowedUserGroups(TwitchUserGroup.Moderator)]
  public static async Task CloseChatCommand(BaseContext _)
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
  [AllowedUserGroups(TwitchUserGroup.Moderator)]
  public static async Task OpenChatCommand(BaseContext _)
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
