using Microsoft.Extensions.Logging;
using Nixill.OBSWS;
using Nixill.OBSWS.Extensions;
using Nixill.Streaming.JoltBot.OBS;

namespace Nixill.Streaming.JoltBot.Scheduled;

public static class AudioMonitoring
{
  // WARNING: THIS ENTIRE CLASS'S CODE IS CURRENTLY UNUSED.
  static readonly ILogger Logger = Log.Factory.CreateLogger(typeof(AudioMonitoring));

  public static async Task Tick()
  {
    await Task.Delay(0);

    while (true)
    {
      if (JoltOBSClient.IsIdentified)
      {
        // One: Reset all audio monitors that are active.
        Logger.LogInformation("Beginning audio monitor reset...");

        // This gets all the inputs currently assigned to OBS.
        // Unfortunately, I don't know a way to get only inputs which have
        // audio.
        IEnumerable<string> inputs = (await OBSRequests.Inputs.GetInputList().Send())
          .Select(i => i.Name);
        Logger.LogDebug($"{inputs.Count()} input(s) found in total.");

        var audioMonitoringStates = await inputs.SelectOBSResults(
          JoltOBSClient.Client,
          i => OBSRequests.Inputs.GetInputAudioMonitorType(i),
          r => (MonitoringType)(OBSSingleValueResult<MonitoringType>)r.RequestResult,
          resultCondition: r => r.RequestSuccessful
        );

        var monitorsToReset = audioMonitoringStates
          .Where(rlt => rlt.Value != MonitoringType.None);
        Logger.LogDebug($"{monitorsToReset.Count()} monitors need to be reset.");

        var disableMonitoringRequests = new OBSRequestBatch(monitorsToReset
          .Select(mtr => OBSRequests.Inputs.SetInputAudioMonitorType(mtr.Key, MonitoringType.None)),
          executionType: RequestBatchExecutionType.SerialRealtime);
        var disableMonitoringResults = await disableMonitoringRequests.Send();
        Logger.LogDebug("Disabled audio monitoring.");

        await Task.Delay(0);

        var enableMonitoringRequests = new OBSRequestBatch(monitorsToReset
          .Select(mtr => OBSRequests.Inputs.SetInputAudioMonitorType(mtr.Key, mtr.Value)),
          executionType: RequestBatchExecutionType.SerialRealtime);
        var enableMonitoringResults = await enableMonitoringRequests.Send();
        Logger.LogInformation("Restored audio monitoring.");

        // Two: Disallow any device from having all six audio tracks
        // enabled. Any that have them all should be changed to have none.
        var trackStates = await audioMonitoringStates.Keys.SelectOBSResults(
          JoltOBSClient.Client,
          i => OBSRequests.Inputs.GetInputAudioTracks(i),
          r => (r.RequestResult as InputAudioTracksResult).AudioTracks,
          resultCondition: r => r.RequestSuccessful
        );

        var tracksToDisable = trackStates
          .Where(kvp => !kvp.Value.Any(tp => tp.Value))
          .Select(kvp => (
            ID: kvp.Key,
            Tracks: kvp.Value.Select(tp => (tp.Key, false)).ToDictionary())
          );

        Logger.LogDebug($"{tracksToDisable.Count()} inputs to disable all audio tracks.");

        var disableAudioTrackRequests = new OBSRequestBatch(tracksToDisable
          .Select(ttd => OBSRequests.Inputs.SetInputAudioTracks(ttd.ID, ttd.Tracks)),
          executionType: RequestBatchExecutionType.SerialRealtime);
        await disableAudioTrackRequests.Send();
        Logger.LogInformation("Finished clearing audio track outputs.");

        // await Task.Delay(TimeSpan.FromSeconds(10));
        await Task.Delay(TimeSpan.FromMinutes(5));
      }
      else
      {
        Logger.LogDebug("Skipping this cycle as OBS is not connected.");
        await Task.Delay(TimeSpan.FromSeconds(10));
      }
    }
  }
}