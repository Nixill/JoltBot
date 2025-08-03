using System.Text;
using System.Text.RegularExpressions;
using Nixill.Colors;
using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.Data;
using Nixill.Streaming.JoltBot.Twitch.Api;
using Nixill.Utils.Extensions;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;

namespace Nixill.Streaming.JoltBot.OBS;

public static partial class GameScreen
{
  [GeneratedRegex(@"\[(\d+)\]")]
  private static partial Regex GamePartNumber();

  internal static async Task SetColorAndBottomText()
  {
    ChannelInformation info = await JoltCache.GetOwnChannelInfo();

    Color color = GamesCsv.GetGameColor(info.GameName);
    await OBSExtraRequests.Inputs.Color.SetColor("clr_BottomFilter", color).Send();

    string gameName = info.GameName;
    string streamTitle = info.Title;
    var streamTitleSplit = streamTitle.Split(" | ");
    string titlePart1 = streamTitleSplit[0];

    StringBuilder titleBuilder = new();

    if (gameName != null && gameName != "") titleBuilder.Append($"{gameName}: ");
    titleBuilder.Append(titlePart1);

    if (GamePartNumber().TryMatch(streamTitle, out Match mtc))
    {
      string number = mtc.Groups[1].Value;
      titleBuilder.Append($" (Part {number})");
    }

    await OBSExtraRequests.Inputs.Text.SetInputText("txt_BottomText", titleBuilder.ToString()).Send();
  }
}