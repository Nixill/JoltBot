using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.Twitch;
using Nixill.Streaming.JoltBot.Twitch.Api;
using Nixill.Utils;
using NodaTime;
using TwitchLib.Api.Helix.Models.Channels.GetAdSchedule;
using TwitchLib.Api.Helix.Models.Channels.StartCommercial;

namespace Nixill.Streaming.JoltBot.OBS;

public static class AdManager
{
  static bool IsInfoKnown = false;
  static Instant _PreRollsExpire = Instant.MinValue;
  static Instant PreRollsExpire
  {
    get
    {
      if (!IsInfoKnown) GetInfo().GetAwaiter().GetResult();
      return _PreRollsExpire;
    }
  }

  public static int NextAdDuration = 180;
  public static bool CountdownRunning = false;

  public static async Task GetInfo()
  {
    AdSchedule answer = (await JoltApiClient.WithToken((api, id) => api.Helix.Channels.GetAdScheduleAsync(id))).Data[0];

    IsInfoKnown = true;

    if (answer.PrerollFreeTime > 0)
      _PreRollsExpire = SystemClock.Instance.GetCurrentInstant() + Duration.FromSeconds(answer.PrerollFreeTime);
  }

  public static int MaximumAdDuration()
  {
    Instant now = SystemClock.Instance.GetCurrentInstant();

    if (now >= PreRollsExpire) return 180;
    int tensOfMinutes = (PreRollsExpire - now).Minutes / 10;
    return 180 - tensOfMinutes * 30;
  }

  public static bool? TryStartAd(int length = 180)
  {
    if (CountdownRunning)
      if (NextAdDuration != length)
      {
        NextAdDuration = length;
        return null;
      }
      else return false;

    Task _ = RunAdAfterCountdown(length);
    return true;
  }

  public static bool TryStopAd()
  {
    if (!CountdownRunning) return false;

    Task _ = StopAdCountdown();
    return true;
  }

  public static async Task RunAdAfterCountdown(int length = 180)
  {
    NextAdDuration = Math.Max(30, Math.Min(180, length / 30 * 30));

    CountdownRunning = true;

    var task_secondsCounterID = OBSRequests.SceneItems.GetSceneItemId("grp_AdCountdown", "txt_AdCountdown").Send();
    var task_currentSceneID = OBSRequests.Scenes.GetCurrentProgramScene().Send();
    Guid currentSceneID = (await task_currentSceneID).Guid;
    int adGroupID = await OBSRequests.SceneItems.GetSceneItemId(currentSceneID, "grp_AdCountdown").Send();
    int secondsCounterID = await task_secondsCounterID;

    var task_adBreakInfo = GetInfo();

    OBSExtraRequests.Inputs.Text.SetText("txt_AdCountdown", "60").SendWithoutWaiting();
    OBSExtraRequests.Inputs.Text.SetText("txt_AdSeconds", "seconds.").SendWithoutWaiting();
    await OBSRequests.SceneItems.SetSceneItemEnabled(currentSceneID, adGroupID, true).Send();

    for (int i = 59; i >= 0; i--)
    {
      await Task.Delay(1000);
      if (!CountdownRunning) return;
      OBSExtraRequests.Inputs.Text.SetText("txt_AdCountdown", i.ToString()).SendWithoutWaiting();
      if (i == 1) OBSExtraRequests.Inputs.Text.SetText("txt_AdSeconds", "second.").SendWithoutWaiting();
      else if (i == 0) OBSExtraRequests.Inputs.Text.SetText("txt_AdSeconds", "seconds.").SendWithoutWaiting();
    }

    OBSRequests.SceneItems.SetSceneItemEnabled(currentSceneID, adGroupID, false).SendWithoutWaiting();

    await task_adBreakInfo;

    // TODO REMEMBER TO SWITCH THIS BACK IN PRODUCTION
    // await JoltChatBot.Chat($"Showing this message instead of running an ad for {Math.Min(NextAdDuration,
    //   MaximumAdDuration())} seconds!");
    _ = JoltApiClient.WithToken((api, id) => api.Helix.Channels.StartCommercialAsync(new StartCommercialRequest
    {
      BroadcasterId = id,
      Length = Math.Min(NextAdDuration, MaximumAdDuration())
    }));

    CountdownRunning = false;
  }

  public static async Task StopAdCountdown()
  {
    if (CountdownRunning)
    {
      CountdownRunning = false;

      var task_currentSceneID = OBSRequests.Scenes.GetCurrentProgramScene().Send();
      Guid currentSceneID = (await task_currentSceneID).Guid;
      int adGroupID = await OBSRequests.SceneItems.GetSceneItemId(currentSceneID, "grp_AdCountdown").Send();

      await OBSRequests.SceneItems.SetSceneItemEnabled(currentSceneID, adGroupID, false).Send();
    }
  }
}