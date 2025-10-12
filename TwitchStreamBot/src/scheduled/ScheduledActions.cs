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
    Task.Run(ClockTicks);
    Task.Run(AudioMonitoring.Tick);
    Task.Run(StreamMemoryClock.Tick);
  }

  public static async Task ClockTicks()
  {
    await Task.Delay(0);

    ZonedDateTimePattern pattern1 = ZonedDateTimePattern.CreateWithInvariantCulture("ddd HH:mm", null);
    ZonedDateTimePattern pattern2 = ZonedDateTimePattern.CreateWithInvariantCulture("ddd uuuu-MM-dd HH:mm", null);
    BclDateTimeZone defaultZone = BclDateTimeZone.ForSystemDefault();

    string lastTimeUpdate = "";

    while (true)
    {
      try
      {
        if (JoltOBSClient.IsIdentified)
        {
          var now = SystemClock.Instance.GetCurrentInstant().InZone(defaultZone);
          string timeNow = pattern1.Format(now);
          if (timeNow != lastTimeUpdate)
          {
            JoltOBSClient.Client.SendBatchRequestWithoutWaiting(new OBSRequestBatch(
              OBSExtraRequests.Inputs.Text.SetText("txt_Clock", timeNow),
              OBSExtraRequests.Inputs.Text.SetText("txt_ClockWithDate", pattern2.Format(now))
            ), executionType: RequestBatchExecutionType.Parallel);
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