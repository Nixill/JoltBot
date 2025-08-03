using Nixill.Streaming.JoltBot.Data;

namespace Nixill.Streaming.JoltBot.Twitch.Commands;

[CommandContainer]
public static class MetaCommands
{
  [Command("command add", "cmd add", "command +", "cmd +")]
  [AllowedUserGroups(TwitchUserGroup.AllModeratorRoles)]
  public static async Task AddCommand(BaseContext ctx, string name, [LongText] string response)
  {
    if (name.StartsWith('!')) name = name[1..];

    bool mod = false;
    bool title = false;

    while (response.StartsWith('-'))
    {
      if (response.StartsWith("-m "))
      {
        mod = true;
        response = response[3..];
      }

      if (response.StartsWith("-t "))
      {
        title = true;
        response = response[3..];
      }

      if (response.StartsWith("-mt ") || response.StartsWith("-tm "))
      {
        mod = true;
        title = true;
        response = response[4..];
      }
    }

    if (CommandsCsv.AddCommand(name, response, mod, title))
      await ctx.ReplyAsync($"Added command \"!{name}\" with response \"{response}\".");
    else
      await ctx.ReplyAsync($"Failed to add command \"!{name}\".");
  }

  [Command("command temp", "cmd temp", "command t", "cmd t")]
  [AllowedUserGroups(TwitchUserGroup.AllModeratorRoles)]
  public static async Task AddTempCommand(BaseContext ctx, string name, [LongText] string response)
  {
    if (name.StartsWith('!')) name = name[1..];

    bool mod = false;
    bool title = false;

    while (response.StartsWith('-'))
    {
      if (response.StartsWith("-m "))
      {
        mod = true;
        response = response[3..];
      }

      if (response.StartsWith("-t "))
      {
        title = true;
        response = response[3..];
      }

      if (response.StartsWith("-mt ") || response.StartsWith("-tm "))
      {
        mod = true;
        title = true;
        response = response[4..];
      }
    }

    if (CommandsCsv.AddTempCommand(name, response, mod, title))
      await ctx.ReplyAsync($"Added temporary command \"!{name}\" with response \"{response}\".");
    else
      await ctx.ReplyAsync($"Failed to add temporary command \"!{name}\".");
  }

  [Command("command remove", "cmd remove", "command -", "cmd -")]
  [AllowedUserGroups(TwitchUserGroup.AllModeratorRoles)]
  public static async Task RemoveCommand(BaseContext ctx, string name)
  {
    if (name.StartsWith('!')) name = name[1..];

    if (CommandsCsv.RemoveCommand(name))
      await ctx.ReplyAsync($"Removed command \"!{name}\".");
    else
      await ctx.ReplyAsync($"Failed to remove command \"!{name}\".");
  }
}