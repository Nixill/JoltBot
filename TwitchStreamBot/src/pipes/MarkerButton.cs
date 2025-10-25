using Microsoft.Extensions.Logging;
using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.OBS;
using Nixill.Streaming.JoltBot.Twitch.Api;
using TwitchLib.Api.Helix.Models.Streams.CreateStreamMarker;

namespace Nixill.Streaming.JoltBot.Pipes;

public static class MarkerButton
{
  public static ILogger Logger = Log.Factory.CreateLogger(typeof(MarkerButton));

  public static Task Place() => Place(null);

  public static async Task Place(string markerText)
  {
    var checkStreaming = OBSRequests.Stream.GetStreamStatus().Send();
    var checkRecording = OBSRequests.Record.GetRecordStatus().Send();

    var streamStatus = await checkStreaming;
    var recordStatus = await checkRecording;

    if (streamStatus.Active && !streamStatus.Reconnecting)
    {
      // Live on Twitch
      var markerCreationTask = JoltApiClient.WithToken((api, id) => api.Helix.Streams.CreateStreamMarkerAsync(new CreateStreamMarkerRequest
      {
        Description = markerText ?? "Created by JoltBot!",
        UserId = id
      }));

      Logger.LogInformation("Placed a stream marker on Twitch!");
    }

    if (recordStatus.Active && !recordStatus.Paused)
    {
      // Recording locally
      Logger.LogInformation("Recording active...");

      // Let's get the current recording filename!
      // This is gonna be tricky because OBS doesn't let us do that except
      // as the recording ends - so instead we'll have to use the
      // recording directory and just find the most recently modified
      // file.
      string dir = await OBSRequests.Config.GetRecordDirectory().Send();
      Logger.LogTrace("Directory: {dir}", dir);

      var dirInfo = new DirectoryInfo(dir);

      var file = dirInfo.GetFiles()
          .Where(f => !f.Name.EndsWith(".markers.txt"))
          .OrderByDescending(f => f.LastWriteTimeUtc)
          .First();
      string filename = file.Name;
      Logger.LogTrace("Filename: {filename}", filename);

      // Now translate it to a marker filename:
      string markerFilename = dir + "\\" + filename[..filename.LastIndexOf('.')] + ".markers.txt";
      Logger.LogTrace("Marker filename: {markerFilename}", markerFilename);

      // Get the current timestamp to write to marker file:
      string time = recordStatus.Timecode;

      // And finally, actually write it!
      File.AppendAllText(markerFilename, time + (markerText != null ? $" - {markerText}" : "") + "\n", System.Text.Encoding.UTF8);
      Logger.LogInformation("Placed a recording marker in local file!");
    }
  }
}