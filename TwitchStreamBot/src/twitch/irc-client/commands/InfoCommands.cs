using System.Text.RegularExpressions;
using Nixill.Streaming.JoltBot.Twitch.Api;

namespace Nixill.Streaming.JoltBot.Twitch.Commands;

[CommandContainer]
public static class InfoCommands
{
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

  // [Command("song")]
  // public static Task SongCommand
}
