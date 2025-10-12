using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.OBS;
using Nixill.Streaming.JoltBot.Twitch;

namespace Nixill.Streaming.JoltBot.Games.UFO50;

[CommandContainer]
public static class BingoInfoUpdater
{
  [Command("ufo50 name")]
  [AllowedUserGroups(TwitchUserGroup.AllModeratorRoles)]
  [AllowedWithGame("UFO 50")]
  [AllowedWithTag("Bingo")]
  public static async Task UpdateNameCommand(CommandContext ctx, int player, [LongText] string name)
  {
    if (player < 1 || player > 2) throw new UserInputException("Player number must be 1 or 2.");
    _ = OBSExtraRequests.Inputs.Text.SetText($"txt_BingoP{player}Name", name).Send();
    _ = ctx.ReplyAsync("Updated!");
  }

  [Command("ufo50 pronouns")]
  [AllowedUserGroups(TwitchUserGroup.AllModeratorRoles)]
  [AllowedWithGame("UFO 50")]
  [AllowedWithTag("Bingo")]
  public static async Task UpdatePronounsCommand(CommandContext ctx, int player, [LongText] string pronouns)
  {
    if (player < 1 || player > 2) throw new UserInputException("Player number must be 1 or 2.");
    _ = OBSExtraRequests.Inputs.Text.SetText($"txt_BingoP{player}Pronouns", pronouns.Replace('/', '\n')).Send();
    _ = ctx.ReplyAsync("Updated!");
  }
}