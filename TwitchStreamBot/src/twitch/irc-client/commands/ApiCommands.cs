using Nixill.Streaming.JoltBot.Twitch.Api;
using Args = Nixill.Streaming.JoltBot.Twitch.CommandContext;

namespace Nixill.Streaming.JoltBot.Twitch.Commands;

[CommandContainer]
public static class ApiCommands
{
  [Command("id")]
  [AllowedGroups(TwitchUserGroup.Moderator)]
  public static async Task IdCommand(Args ev, string username)
  {
    username = username.ToLowerInvariant();

    var userId = await JoltHelperMethods.GetUserId(username);

    if (userId != null)
      await ev.ReplyAsync($"{username}'s ID is {userId}");
    else
      await ev.ReplyAsync($"{username} couldn't be found.");
  }

  [Command("shoutout", "so")]
  [AllowedGroups(TwitchUserGroup.Moderator)]
  public static async Task ShoutoutCommand(Args ev, string username)
  {
    var userId = await JoltHelperMethods.GetUserId(username.ToLowerInvariant());

    if (userId == null)
    {
      await ev.ReplyAsync($"{username} couldn't be found.");
      return;
    }

    Task sendShoutout = JoltApiClient.WithToken((api, id) =>
      api.Helix.Chat.SendShoutoutAsync(id, userId, id));

    var userInfo = (await JoltApiClient.WithToken(api => api.Helix.Channels.GetChannelInformationAsync(userId)))
      .Data.Where(ui => ui.BroadcasterId == userId).First();

    Task sendMessage = ev.ReplyAsync($"Yo, go check out {userInfo.BroadcasterName}, last seen playing {(
      userInfo.GameName)}! → https://twitch.tv/{userInfo.BroadcasterLogin}");

    await sendShoutout;
    await sendMessage;
  }

  [Command("shoutout nixillshadowfox", "so nixillshadowfox")]
  public static async Task ShoutoutMyself(Args ev)
    => await ev.ReplyAsync("Of course you should check out my streams!");
}