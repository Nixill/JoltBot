using Microsoft.Extensions.Logging;
using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.JSON;

namespace Nixill.Streaming.JoltBot.OBS;

public static class JoltOBSClient
{
  internal static OBSClient Client;
  static ILogger Logger = Log.Factory.CreateLogger(typeof(JoltOBSClient));

  public static bool IsConnected => Client.IsConnected;

  public static async Task SetUp()
  {
    Client = new(OBSJson.Server.IP, OBSJson.Server.Port, OBSJson.Server.Password, loggerFactory: Log.Factory);

    Client.Events.Outputs.VirtualcamStarted += AdsAtStartOfStream;

    await Client.Connect();
  }

  private static void AdsAtStartOfStream(object sender, OutputStateChanged e)
  {
    if (!AdManager.CountdownRunning) { Task _ = AdManager.RunAdAfterCountdown(); }
  }

  public static Task<T> Send<T>(this OBSRequest<T> request, int timeout = 300) where T : OBSRequestResult
  => Client.SendRequest(request);

  public static Task Send(this OBSVoidRequest request, int timeout = 300)
    => Client.SendRequest(request);

  public static void SendWithoutWaiting(this OBSRequest request)
    => Client.SendRequestWithoutWaiting(request);
}