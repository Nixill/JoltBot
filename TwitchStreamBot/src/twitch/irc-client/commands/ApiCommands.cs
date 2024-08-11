using Nixill.Streaming.JoltBot.Twitch.Api;

namespace Nixill.Streaming.JoltBot.Twitch.Commands;

[CommandContainer]
public static class ApiCommands
{
  [Command("id")]
  [AllowedUserGroups(TwitchUserGroup.Moderator)]
  public static async Task IdCommand(BaseContext ctx, string username)
  {
    username = username.ToLowerInvariant();

    var userId = await JoltHelperMethods.GetUserId(username);

    if (userId != null)
      await ctx.ReplyAsync($"{username}'s ID is {userId}");
    else
      await ctx.ReplyAsync($"{username} couldn't be found.");
  }

  [Command("shoutout", "so")]
  [AllowedUserGroups(TwitchUserGroup.Moderator)]
  public static async Task ShoutoutCommand(BaseContext ctx, string username)
  {
    var userId = await JoltHelperMethods.GetUserId(username.ToLowerInvariant());

    if (userId == null)
    {
      await ctx.ReplyAsync($"{username} couldn't be found.");
      return;
    }

    Task sendShoutout = JoltApiClient.WithToken((api, id) =>
      api.Helix.Chat.SendShoutoutAsync(id, userId, id));

    var userInfo = (await JoltApiClient.WithToken(api => api.Helix.Channels.GetChannelInformationAsync(userId)))
      .Data.Where(ui => ui.BroadcasterId == userId).First();

    Task sendMessage = ctx.ReplyAsync($"Yo, go check out {userInfo.BroadcasterName}, last seen playing {(
      userInfo.GameName)}! → https://twitch.tv/{userInfo.BroadcasterLogin}");

    await sendShoutout;
    await sendMessage;
  }

  [Command("shoutout nixillshadowfox", "so nixillshadowfox")]
  // When I implement an integrated commands list, this command should be
  // hidden from it.
  // [HideFromList]
  public static async Task ShoutoutMyself(BaseContext ctx)
    => await ctx.ReplyAsync("Of course you should check out my streams!");
}