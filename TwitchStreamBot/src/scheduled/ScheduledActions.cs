using System.Text.Json.Nodes;
using Nixill.Streaming.JoltBot.OBS;
using NodaTime;
using NodaTime.Text;
using NodaTime.TimeZones;

namespace Nixill.Streaming.JoltBot.Scheduled;

public class ScheduledActions
{
  public static void RunAll()
  {
    Task _ = ClockTick();
  }

  public static Task ClockTick()
  {
    ZonedDateTimePattern pattern = ZonedDateTimePattern.CreateWithInvariantCulture("ddd HH:mm:ss", null);
    BclDateTimeZone defaultZone = BclDateTimeZone.ForSystemDefault();

    string lastTimeUpdate = "";

    while (true)
    {
      if (JoltOBSClient.IsConnected)
      {
        string timeNow = pattern.Format(SystemClock.Instance.GetCurrentInstant().InZone(defaultZone));
        if (timeNow != lastTimeUpdate)
        {
          JoltOBSClient.Client.SendRequest("SetInputSettings", new JsonObject
          {
            ["inputName"] = "txt_Clock",
            ["inputSettings"] = new JsonObject
            {
              ["text"] = timeNow
            }
          });
          lastTimeUpdate = timeNow;
        }
      }

      Thread.Sleep(50);
    }
  }
}