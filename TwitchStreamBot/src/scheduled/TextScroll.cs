using System.Reflection;
using Microsoft.Extensions.Logging;
using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.Files;
using Nixill.Streaming.JoltBot.OBS;
using Nixill.Streaming.JoltBot.Twitch;
using Nixill.Streaming.JoltBot.Twitch.Api;
using Nixill.Utils;

namespace Nixill.Streaming.JoltBot.Scheduled;

public static class TextScroll
{
  static readonly ILogger Logger = Log.Factory.CreateLogger(typeof(TextScroll));

  static string Text = "";
  static int DelayFrames = 0;

  const int FramesPerAnim = 6;

  #region Tick function
  public static async Task Tick()
  {
    // let things get set up first
    await Task.Delay(5_000);

    List<MemberInfo> Texts = LoadTexts();
    // await OBSExtraRequests.Inputs.Text.SetInputText("txt_BottomText", " " + $"{lastText}{new string(' ', 36)}"[0..36]).Send();

    while (true)
    {
      (Text, DelayFrames) = await GetNextTextToShow(Texts);

      #region Bring text onscreen
      string preSpacedText = $"{new string(' ', 36)}{Text}{(Text.Length < 36 ? new string(' ', 36 - Text.Length) : "")}";
      var enterTransition = Enumerable.Range(1, 36).Select(i => preSpacedText[i..(i + 36)]);
      await new OBSRequestBatch(enterTransition.SelectMany(t => new OBSRequest[] {
        OBSExtraRequests.Inputs.Text.SetInputText("txt_BottomText", " " + t),
        OBSRequests.General.Sleep(frames: FramesPerAnim)
      }), executionType: RequestBatchExecutionType.SerialFrame).Send();
      await Task.Delay(200);
      #endregion

      #region Await text timer
      var widths = Enumerable.Range(1, DelayFrames).Select(i => i * 1880 / DelayFrames);
      await new OBSRequestBatch(widths.SelectMany(w => new OBSRequest[] {
        OBSExtraRequests.Inputs.Color.SetSize("clr_TimerLine", w, 2),
        OBSRequests.General.Sleep(frames: 1)
      }), executionType: RequestBatchExecutionType.SerialFrame).Send();
      #endregion

      #region Send text offscreen
      string postSpacedText = $"{Text}{new string(' ', 36)}";
      var exitTransition = Enumerable.Range(1, Text.Length).Select(i => postSpacedText[i..(i + 36)]);
      await new OBSRequestBatch(exitTransition.SelectMany(t => new OBSRequest[] {
        OBSExtraRequests.Inputs.Text.SetInputText("txt_BottomText", " " + t),
        OBSRequests.General.Sleep(frames: FramesPerAnim)
      }), executionType: RequestBatchExecutionType.SerialFrame).Send();
      await Task.Delay(200);
      #endregion
    }
  }
  #endregion

  static async Task<(string, int)> GetNextTextToShow(List<MemberInfo> texts)
  {
    var channelInfo = await JoltCache.GetOwnChannelInfo();

    int left = texts.Count * 2;

    while (left-- > 0)
    {
      MemberInfo member = texts.Pop();
      texts.Add(member);

      bool enable = true;
      foreach (var restriction in member.GetCustomAttributes<LimitAttribute>())
      {
        bool? passResult = await restriction.PassesCondition(TickerTextCheckContext.Instance, channelInfo);
        if (passResult == true)
        {
          enable = true;
          if (restriction.StopOnAllow) break;
        }
        else if (passResult == false)
        {
          enable = false;
          if (restriction.StopOnDeny) break;
        }
      }

      if (!enable) continue;

      TickerTextAttribute attr = member.GetCustomAttribute<TickerTextAttribute>();
      string value = null;

      if (member is FieldInfo field)
      {
        if (field.FieldType != typeof(string)) continue;
        if (!field.IsStatic) continue;
        value = (string)field.GetValue(null);
      }
      else if (member is PropertyInfo property)
      {
        if (property.PropertyType != typeof(string)) continue;
        if (!property.GetGetMethod().IsStatic) continue;
        value = (string)property.GetValue(null);
      }
      else if (member is MethodInfo method)
      {
        if (!method.IsStatic) continue;
        if (method.GetParameters().Length != 0) continue;
        if (method.ReturnType == typeof(string))
        {
          value = (string)method.Invoke(null, []);
        }
        else if (method.ReturnType == typeof(Task<string>))
        {
          value = await (Task<string>)method.Invoke(null, []);
        }
      }

      if (value != null) return (value, attr.DelayTotalFrames);
    }

    // Fallback:
    return (Text, DelayFrames);
  }

  static List<MemberInfo> LoadTexts()
    => typeof(TextsToShow).GetMembers().Where(m => m.GetCustomAttribute<TickerTextAttribute>() != null)
      .OrderByAttribute().ToList();
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
public class TickerTextAttribute(int seconds = 0, int frames = 0) : Attribute
{
  public int DelaySeconds = seconds;
  public int DelayFrames = frames;

  public int DelayTotalFrames => DelaySeconds * 30 + DelayFrames;
}

public static class TextsToShow
{
  // [TickerText(0)]
  [TickerText(30)]
  // [TickerText(5)]
  [Order(1)]
  public static async Task<string> GetGameText()
  {
    var info = await JoltCache.GetOwnChannelInfo();
    var pieces = info.Title.Split(" | ");
    var title = pieces[0];
    var game = pieces[1];
    return $"{game}: {title}";
  }

  // [TickerText(0)]
  [TickerText(10)]
  // [TickerText(5)]
  [Order(2)]
  public const string JoinTheDiscord = "Join the discord server! -> https://discord.nixill.net/";

  [TickerText(10)]
  // [TickerText(5)]
  [Order(3)]
  public static string PretzelText => PretzelFileWatcher.ShowText();

  [TickerText(0)]
  [ChanceToAppear(0.001)]
  [Order(100)]
  public const string YipYap = "Yip yap!";
}