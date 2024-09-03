using Microsoft.Extensions.Logging;
using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.Data;
using Nixill.Streaming.JoltBot.OBS;
using NodaTime;

namespace Nixill.Streaming.JoltBot.Scheduled;

public static class StreamMemoryClock
{
  static readonly ILogger Logger = Log.Factory.CreateLogger(typeof(StreamMemoryClock));

  static Instant Now => SystemClock.Instance.GetCurrentInstant();

  public static async Task Tick()
  {
    while (!JoltOBSClient.IsIdentified)
    {
      await Task.Delay(TimeSpan.FromSeconds(10));
    }

    bool isStreaming = await OBSRequests.Stream.GetStreamStatus().Send().ContinueWith(task => task.Result.Active);
    bool wasStreaming = MemoryJson.Clock.LastKnownState;

    if (isStreaming != wasStreaming)
    {
      if (!isStreaming) MemoryJson.Clock.LastEndTime = MemoryJson.Clock.LastKnownTime;
      else MemoryJson.Clock.LastStartTime = Now;
    }

    MemoryJson.Clock.LastKnownState = isStreaming;

    JoltOBSClient.Client.Events.Outputs.StreamStarted += (sender, args) =>
    {
      MemoryJson.Clock.LastStartTime = Now;
      MemoryJson.Save();
    };

    JoltOBSClient.Client.Events.Outputs.StreamStopped += (sender, args) =>
    {
      MemoryJson.Clock.LastEndTime = Now;
      MemoryJson.Save();
    };

    while (true)
    {
      MemoryJson.Clock.LastKnownTime = Now;
      MemoryJson.Save();

      await Task.Delay(TimeSpan.FromMinutes(1));
    }
  }
}