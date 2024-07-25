using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TwitchLib.EventSub.Websockets;
using TwitchLib.EventSub.Websockets.Client;
using TwitchLib.EventSub.Websockets.Core.Handler;
using TwitchLib.EventSub.Websockets.Extensions;

namespace Nixill.Streaming.JoltBot.Twitch.Events;

public static class JoltEventClient
{
  static ILogger Logger = Log.Factory.CreateLogger(typeof(JoltEventClient));

  public static Task SetUp()
  {
    Host.CreateDefaultBuilder()
      .ConfigureServices((hostContext, services) =>
      {
        services.AddTwitchLibEventSubWebsockets()
          .AddHostedService<JoltEventService>();
      }).Build().Run();
    return Task.CompletedTask;
  }
}