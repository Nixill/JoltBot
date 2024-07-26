using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Nixill.Streaming.JoltBot.OBS;

namespace Nixill.Streaming.JoltBot.Pipes;

public static class PipeRunner
{
  static ILogger Logger = Log.Factory.CreateLogger(typeof(PipeRunner));

  public static void SetUp()
  {
    Task _ = Task.Run(PipeServer.ListenForMessages);

    PipeServer.MessageReceived += (sender, args) =>
    {
      JsonObject data = args.Data;
      Logger.LogInformation($"Received message from pipe: {data.ToJsonString()}");

      string req = (string)data["request"];

      switch (req)
      {
        case null: break;
        case "Ad.Start":
          AdManager.TryStartAd((int?)data["length"] ?? 180);
          break;
        case "Ad.Stop": AdManager.TryStopAd(); break;
      }
    };
  }
}