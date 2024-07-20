using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nixill.Streaming.JoltBot.Twitch.Api;
using TwitchLib.EventSub.Websockets;
using TwitchLib.EventSub.Websockets.Client;
using TwitchLib.EventSub.Websockets.Core.EventArgs;
using TwitchLib.EventSub.Websockets.Core.EventArgs.Channel;

namespace Nixill.Streaming.JoltBot.Twitch.Events;

public class JoltEventService : IHostedService
{
  ILogger Logger = Log.Factory.CreateLogger("JoltEventService");
  EventSubWebsocketClient Client;

  public JoltEventService(ILogger<JoltEventService> _, EventSubWebsocketClient client)
  {
    Client = client ?? throw new ArgumentNullException(nameof(client));

    Client.WebsocketConnected += OnConnect;
    Client.ChannelChatMessage += OnChatMessage;
  }

  private async Task OnConnect(object sender, WebsocketConnectedArgs e)
  {
    Logger.LogInformation("Connected.");

    if (!e.IsRequestedReconnect)
    {
      await JoltApiClient.WithToken(api => api.Helix.EventSub.CreateEventSubSubscriptionAsync("channel.chat.message", "1", new()
      {
        ["broadcaster_user_id"] = JoltTwitchMain.Channel.UserId,
        ["user_id"] = JoltTwitchMain.Channel.UserId
      }, TwitchLib.Api.Core.Enums.EventSubTransportMethod.Websocket, Client.SessionId));
    }
  }

  private async Task OnChatMessage(Object sender, ChannelChatMessageArgs ev)
  {
    Logger.LogInformation("Chat message received.");
    await Task.Delay(0);
  }

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    await Client.ConnectAsync();
  }

  public async Task StopAsync(CancellationToken cancellationToken)
  {
    await Client.DisconnectAsync();
  }
}