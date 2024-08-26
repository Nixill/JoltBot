
using Nixill.Colors;
using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.Data;
using Nixill.Streaming.JoltBot.Twitch.Api;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;

namespace Nixill.Streaming.JoltBot.OBS;

public static class GameScreen
{
  internal static async Task SetColor()
  {
    ChannelInformation info = await JoltCache.GetOwnChannelInfo();
    Color color = GamesCsv.GetGameColor(info.GameName);
    await OBSExtraRequests.Inputs.Color.SetColor("clr_BottomFilter", color).Send();
  }
}