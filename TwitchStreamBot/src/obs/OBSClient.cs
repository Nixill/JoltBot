using Microsoft.Extensions.Logging;
using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.Data;

namespace Nixill.Streaming.JoltBot.OBS;

public static class JoltOBSClient
{
  const int GlobalTimeout = 3600;
  internal static OBSClient Client;
  static ILogger Logger = Log.Factory.CreateLogger(typeof(JoltOBSClient));

  public static bool IsConnected => Client.IsConnected;
  public static bool IsIdentified => Client.IsIdentified;

  public static async Task SetUp()
  {
    Client = new(OBSJson.Server.IP, OBSJson.Server.Port, OBSJson.Server.Password, loggerFactory: Log.Factory);

    Client.Events.Outputs.StreamStarted += (s, e) => Task.Run(() => JoltOBSEventHandlers.StreamStarted(s, e));
    Client.Events.Outputs.StreamStopped += (s, e) => Task.Run(() => JoltOBSEventHandlers.StreamStopped(s, e));

    await Client.Connect();
  }

  public static Task<T> Send<T>(this OBSRequest<T> request, int timeout = GlobalTimeout) where T : OBSRequestResult
    => Client.SendRequest(request, timeout);

  public static Task Send(this OBSVoidRequest request, int timeout = GlobalTimeout)
    => Client.SendRequest(request, timeout);

  public static void SendWithoutWaiting(this OBSRequest request)
    => Client.SendRequestWithoutWaiting(request);

  public static Task<OBSRequestBatchResult> Send(this OBSRequestBatch requests, int timeout = GlobalTimeout)
    => Client.SendBatchRequest(requests, timeout: timeout);

  public static void SendWithoutWaiting(this OBSRequestBatch requests)
    => Client.SendBatchRequestWithoutWaiting(requests);
}