using System.Text.RegularExpressions;
using Nixill.OBSWS;
using Nixill.OBSWS.Utils;
using Nixill.Streaming.JoltBot.Data.UFO50;
using Nixill.Streaming.JoltBot.OBS;
using Nixill.Streaming.JoltBot.Twitch;
using Nixill.Streaming.JoltBot.Twitch.Events;
using Nixill.Streaming.JoltBot.Twitch.Events.Rewards;

namespace Nixill.Streaming.JoltBot.Games.UFO50;

[RewardContainer]
public static partial class BingoBackgroundChanger
{
  [GeneratedRegex(@"(blood|blue ?sky|ceramic|cherry|gold|golf|indigo|infinity|lemon|tangerine)", RegexOptions.IgnoreCase)]
  private static partial Regex BackgroundNames { get; }

  [ChannelPointsReward("UFO50.ChangeBackground")]
  [AllowedWithGame("UFO 50")]
  [AllowedWithTag("Bingo")]
  public static async Task ChangeBackgroundReward(RewardContext ctx)
  {
    MatchCollection mtcs = BackgroundNames.Matches(ctx.Message);
    string[] values = [.. mtcs.Select(m => m.Value).Distinct(StringComparer.InvariantCultureIgnoreCase)];

    if (values.Length == 0)
    {
      await ctx.ReplyAsync($"No valid background found in this message! I'll refund your points...");
      await JoltRewardResponse.RejectRewardByName("UFO50.ChangeBackground", ctx.RedemptionArgs.Id);
      return;
    }

    if (values.Length > 1)
    {
      await ctx.ReplyAsync($"You named too many backgrounds! I can only set one. I'll refund your points...");
      await JoltRewardResponse.RejectRewardByName("UFO50.ChangeBackground", ctx.RedemptionArgs.Id);
      return;
    }

    string bgName = values[0];

    if (bgName.Equals("bluesky", StringComparison.InvariantCultureIgnoreCase)) bgName = "blue sky";

    UFO50Background bg = UFO50BackgroundsCsv.GetBackground(ctx.Message);

    await new OBSRequestBatch(
      OBSExtraRequests.Inputs.Image.SetInputImage("img_UFOBackground",
        @$"C:\Users\Nixill\Documents\Streaming-2024\Images\UFO50\Library_Backgrounds\{bg.Name}.png"),
      OBSExtraRequests.Inputs.Color.SetColor("clr_Bingo-UI-BG_410x60", ColorConversions.FromRGBA(bg.ScreenColor)),
      OBSExtraRequests.Inputs.Color.SetColor("clr_Bingo-UI-BG_410x100", ColorConversions.FromRGBA(bg.ScreenColor)),
      OBSExtraRequests.Inputs.Color.SetColor("clr_Bingo-UI-BG 440x120", ColorConversions.FromRGBA(bg.ScreenColor)),
      OBSExtraRequests.Filters.ColorCorrection.SetMultipliedColor("grp_BingoUIColors",
        "cc_Bingo Background Coloration", ColorConversions.FromRGB(bg.UIColor)),
      OBSExtraRequests.Filters.ColorCorrection.SetMultipliedColor("grp_BingoUIColors2",
        "cc_Bingo Background Coloration", ColorConversions.FromRGB(bg.UIColor)),
      OBSExtraRequests.Filters.ColorCorrection.SetMultipliedColor("grp_BingoUIColors3",
        "cc_Bingo Background Coloration", ColorConversions.FromRGB(bg.UIColor))
    ).Send();

    await JoltRewardResponse.CompleteRewardByName("UFO50.ChangeBackground", ctx.RedemptionArgs.Id);
  }
}