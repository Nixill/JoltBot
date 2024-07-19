namespace Nixill.Streaming.JoltBot.Twitch.Api;

public static class JoltHelperMethods
{
  public static async Task<string> GetUserId(string username)
  {
    username = username.ToLowerInvariant();

    var userResponse = (await JoltApiClient.WithToken(api =>
      api.Helix.Users.GetUsersAsync(logins: new List<string> { username })))
      .Users.Where(u => u.Login == username);

    if (userResponse.Any())
      return userResponse.First().Id;
    else
      return null;
  }

  public static string GetOwnUserId()
    => JoltTwitchMain.Channel.UserId;
}