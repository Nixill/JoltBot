using Microsoft.Extensions.Logging;
using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.OBS;
using Nixill.Streaming.JoltBot.Twitch.Api;
using Nixill.Utils;

namespace Nixill.Streaming.JoltBot.Scheduled;

public static class TextScroll
{
  static readonly ILogger Logger = Log.Factory.CreateLogger(typeof(TextScroll));

  internal static string[] OtherTexts = ["Join the discord! -> https://discord.nixill.net/"];

  public static async Task Tick()
  {
    // let things get set up first
    await Task.Delay(5_000);

    List<string> texts = [];
    string lastText = await GetGameText();
    await OBSExtraRequests.Inputs.Text.SetInputText("txt_BottomText", lastText).Send();

    while (true)
    {
      await Task.Delay(10_000);

      if (texts.Count == 0)
      {
        await Task.Delay(50_000);
        texts = [.. OtherTexts, await GetGameText()];
      }

      string nextText = texts.Pop();

      string RepeatedText = $"{lastText}{new string(' ', 36)}{nextText}{new string(' ', 36)}";
      var TransTexts = Enumerable.Range(1, lastText.Length + 36).Select(i => RepeatedText[i..(i + 36)]);

      await new OBSRequestBatch(TransTexts.SelectMany(t => new OBSRequest[] {
        OBSExtraRequests.Inputs.Text.SetInputText("txt_BottomText", t),
        OBSRequests.General.Sleep(frames: 6)
      }), executionType: RequestBatchExecutionType.SerialFrame).Send();

      lastText = nextText;
    }
  }

  public static async Task<string> GetGameText()
  {
    var info = await JoltCache.GetOwnChannelInfo();
    var pieces = info.Title.Split(" | ");
    var title = pieces[0];
    var game = pieces[1];
    return $"{game}: {title}";
  }
}