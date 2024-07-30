using Nixill.Streaming.JoltBot.JSON;
using Nixill.Streaming.JoltBot.OBS;
using Nixill.Streaming.JoltBot.Twitch.Api;
using TwitchLib.Api.Helix.Models.Chat.ChatSettings;
using Args = TwitchLib.Client.Events.OnChatCommandReceivedArgs;

namespace Nixill.Streaming.JoltBot.Twitch.Commands;

[CommandContainer]
public static class ModCommands
{
  [Command("closechat")]
  [AllowedGroups(TwitchUserGroup.Moderator)]
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
  [AllowedGroups(TwitchUserGroup.Moderator)]
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

  [Command("raid")]
  [AllowedGroups(TwitchUserGroup.Moderator)]
  public static async Task RaidCommand(Args ev, string user)
  {
    var targetInfo = (await
      JoltApiClient.WithToken((api, id) => api
        .Helix
        .Users
        .GetUsersAsync(logins: new List<string> { user })))
      .Users
      .First(ui => ui.Login.ToLower() == user.ToLower());

    var targetId = targetInfo.Id;
    var targetPfp = targetInfo.ProfileImageUrl;

    await JoltApiClient.WithToken((api, id) => api.Helix.Raids.StartRaidAsync(id, targetId));

    await EndScreenManager.PrepareRaid(user, targetPfp);
  }
}
