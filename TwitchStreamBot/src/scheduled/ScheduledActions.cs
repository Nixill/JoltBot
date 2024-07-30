using System.Text.Json.Nodes;
using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.OBS;
using NodaTime;
using NodaTime.Text;
using NodaTime.TimeZones;

namespace Nixill.Streaming.JoltBot.Scheduled;

public class ScheduledActions
{
  public static void RunAll()
  {
    Task.Run(ClockTick);
    Task.Run(AudioMonitoring.Tick);
  }

  public static async Task ClockTick()
  {
    await Task.Delay(0);

    ZonedDateTimePattern pattern = ZonedDateTimePattern.CreateWithInvariantCulture("ddd HH:mm:ss", null);
    BclDateTimeZone defaultZone = BclDateTimeZone.ForSystemDefault();

    string lastTimeUpdate = "";

    while (true)
    {
      try
      {
        if (JoltOBSClient.IsIdentified)
        {
          string timeNow = pattern.Format(SystemClock.Instance.GetCurrentInstant().InZone(defaultZone));
          if (timeNow != lastTimeUpdate)
          {
            Task _ = JoltOBSClient.Client.SendRequestWithoutWaiting(OBSExtraRequests.Inputs.Text.SetInputText("txt_Clock", timeNow));
            lastTimeUpdate = timeNow;
          }
        }
      }
      catch (RequestTimedOutException)
      {
        // do nothing, just try again
      }

      Thread.Sleep(50);
    }
  }
}