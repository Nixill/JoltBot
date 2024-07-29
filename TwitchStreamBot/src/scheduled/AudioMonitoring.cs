using Microsoft.Extensions.Logging;
using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.OBS;

namespace Nixill.Streaming.JoltBot.Scheduled;

public static class AudioMonitoring
{
  public static ILogger Logger = Log.Factory.CreateLogger(typeof(AudioMonitoring));

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

        OBSRequestBatch getAudioRequests = new OBSRequestBatch(
          inputs.Select(id => OBSRequests.Inputs.GetInputAudioMonitorType(id)),
          executionType: RequestBatchExecutionType.SerialRealtime);
        OBSRequestBatchResult audioResults = await getAudioRequests.Send();
        var monitorsToReset = audioResults
          .Where(rsp => rsp.RequestSuccessful)
          .Select(rsp => rsp.RequestResult as OBSSingleValueResult<MonitoringType>)
          .Where(rlt => rlt != null)
          .Select(rlt => (ID: (string)rlt.OriginalRequest.RequestData["inputName"], Current: rlt.Result))
          .Where(rlt => rlt.Current != MonitoringType.None);
        Logger.LogDebug($"{monitorsToReset.Count()} monitors need to be reset.");

        var disableMonitoringRequests = new OBSRequestBatch(monitorsToReset
          .Select(mtr => OBSRequests.Inputs.SetInputAudioMonitorType(mtr.ID, MonitoringType.None)),
          executionType: RequestBatchExecutionType.SerialRealtime);
        var disableMonitoringResults = await disableMonitoringRequests.Send();
        Logger.LogDebug("Disabled audio monitoring.");

        var enableMonitoringRequests = new OBSRequestBatch(monitorsToReset
          .Select(mtr => OBSRequests.Inputs.SetInputAudioMonitorType(mtr.ID, mtr.Current)),
          executionType: RequestBatchExecutionType.SerialRealtime);
        var enableMonitoringResults = await enableMonitoringRequests.Send();
        Logger.LogInformation("Restored audio monitoring.");

        // Two: Disallow any device from having all six audio tracks
        // enabled. Any that have them all should be changed to have none.
        var trackRequests = new OBSRequestBatch(inputs
          .Select(id => OBSRequests.Inputs.GetInputAudioTracks(id)), executionType: RequestBatchExecutionType.SerialRealtime);
        OBSRequestBatchResult trackResults = await trackRequests.Send();
        var tracksToDisable = trackResults
          .Where(rsp => rsp.RequestSuccessful)
          .Select(rsp => rsp.RequestResult as InputAudioTracksResult)
          .Where(rlt => rlt != null && !rlt.AudioTracks.Any(kvp => !kvp.Value))
          .Select(rlt => (
            ID: (string)rlt.OriginalRequest.RequestData["inputName"],
            Tracks: rlt.AudioTracks.Select(kvp => (kvp.Key, false)).ToDictionary())
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