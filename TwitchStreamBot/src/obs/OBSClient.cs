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

    await Client.Connect();
  }
}