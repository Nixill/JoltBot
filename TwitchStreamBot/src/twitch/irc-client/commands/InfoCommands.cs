using System.Text.RegularExpressions;
using Nixill.Streaming.JoltBot.Twitch.Api;

namespace Nixill.Streaming.JoltBot.Twitch.Commands;

[CommandContainer]
public static class InfoCommands
{
  [Command("discord")]
  public static Task DiscordCommand(BaseContext ctx)
    => ctx.ReplyAsync("Join the Shadow Den discord server! → https://discord.nixill.net/");

  [Command("images")]
  public static Task ImagesCommand(BaseContext ctx)
    => ctx.ReplyAsync("See images and names here → https://imgur.com/a/CvUnjC0");

  public static readonly Regex username = new Regex(@"@([A-Za-z0-9][A-Za-z0-9_]{0,24})");

  [Command("multi", "multistream")]
  public static async Task MultiCommand(BaseContext ctx)
  {
    string title = (await JoltCache.GetOwnChannelInfo()).Title;

    string multiOutput = "";
    int count = 0;

    foreach (Match match in username.Matches(title))
    {
      multiOutput += $"/{match.Groups[1].Value}";
      count++;
    }

    if (count >= 1)
    {
      await ctx.ReplyAsync($"https://multi.nixill.net{multiOutput}{count switch
      {
        1 => "/layout4",
        2 => "/layout7",
        3 => "/layout11",
        4 => "/layout15",
        5 => "/layout17",
        6 => "/layout19",
        7 => "/layout22",
        _ => ""
      }}");
    }
    else
    {
      await ctx.ReplyAsync("No multistream tonight!");
    }
  }

  [Command("overscan")]
  [AllowedWithTitle("!overscan")]
  public static Task OverscanCommand(BaseContext ctx)
    => ctx.ReplyAsync("I'm using overscan compensation! I can see more of the game than is visible on stream;"
      + " but the view on stream is larger for readability.");

  [Command("racing")]
  [AllowedWithTitle("!racing")]
  public static Task RacingCommand(BaseContext ctx)
    => ctx.ReplyAsync("I'm on the bottom. @GeorgeMHall is top left. @Jeynick is top right.");

  [Command("pronouns")]
  public static Task PronounsCommand(BaseContext ctx)
    => ctx.ReplyAsync("Nixill's pronouns are they/she (or anything except he or it)! If you visit https://pr.alejo.io/,"
      + " you can get an extension to view people's pronouns or set them for other users of the extension.");

  // [Command("song")]
  // public static Task SongCommand
}
