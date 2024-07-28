using Microsoft.Extensions.Logging;
using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.JSON;

namespace Nixill.Streaming.JoltBot.OBS;

public static class JoltOBSClient
{
  internal static OBSClient Client;
  static ILogger Logger = Log.Factory.CreateLogger(typeof(JoltOBSClient));

  public static bool IsConnected => Client.IsConnected;
  public static bool IsIdentified => Client.IsIdentified;

  public static async Task SetUp()
  {
    Client = new(OBSJson.Server.IP, OBSJson.Server.Port, OBSJson.Server.Password, loggerFactory: Log.Factory);

    Client.Events.Outputs.StreamStarted += (s, e) => Task.Run(() => JoltOBSEventHandlers.StreamStarted(s, e));

    await Client.Connect();
  }

  public static Task<T> Send<T>(this OBSRequest<T> request, int timeout = 30) where T : OBSRequestResult
    => Client.SendRequest(request);

  public static Task Send(this OBSVoidRequest request, int timeout = 30)
    => Client.SendRequest(request);

  public static void SendWithoutWaiting(this OBSRequest request)
    => Client.SendRequestWithoutWaiting(request);

  public static Task<OBSBatchRequestResult> Send(this IEnumerable<OBSRequest> requests, int timeout = 30)
    => Client.SendBatchRequest(requests);

  public static void SendWithoutWaiting(this IEnumerable<OBSRequest> requests)
    => Client.SendBatchRequestWithoutWaiting(requests);
}