using Microsoft.Extensions.Logging;

namespace Nixill.Streaming.JoltBot.Pipes;

public static class PipeRunner
{
  static ILogger Logger = Log.Factory.CreateLogger(typeof(PipeRunner));

  public static void SetUp()
  {
    Task _ = Task.Run(PipeServer.ListenForMessages);

    PipeServer.MessageReceived += (sender, args) =>
    {
      Logger.LogInformation($"Received message from pipe: {args.Data}");
    };
  }
}