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

    if (count == 0)
    {
      await ctx.ReplyAsync("No multistream tonight!");
    }
    else if (count == 1)
    {
      await ctx.ReplyAsync($"https://multi.nixill.net{multiOutput}/layout4");
    }
    else
    {
      await ctx.ReplyAsync($"https://multi.nixill.net{multiOutput}");
    }
  }

  [Command("pronouns")]
  public static Task PronounsCommand(BaseContext ctx)
    => ctx.ReplyAsync("Nixill's pronouns are they/she (or anything except he or it)! If you visit https://pr.alejo.io/,"
      + " you can get an extension to view people's pronouns or set them for other users of the extension.");

  // [Command("song")]
  // public static Task SongCommand
}
