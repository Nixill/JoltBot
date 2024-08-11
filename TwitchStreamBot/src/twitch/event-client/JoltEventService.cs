using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nixill.Streaming.JoltBot.OBS;
using Nixill.Streaming.JoltBot.Twitch.Api;
using TwitchLib.EventSub.Websockets;
using TwitchLib.EventSub.Websockets.Core.EventArgs;
using TwitchLib.EventSub.Websockets.Core.EventArgs.Channel;

namespace Nixill.Streaming.JoltBot.Twitch.Events;

public class JoltEventService : IHostedService
{
  ILogger Logger = Log.Factory.CreateLogger(typeof(JoltEventService));
  EventSubWebsocketClient Client;
  List<EventSubArgs> EventsToSubscribe = new();

  internal async Task RegisterEventSub(string eventType, string version,
    params KeyValuePair<string, string>[] conditions)
  {
    await JoltApiClient.WithToken(api => api.Helix.EventSub.CreateEventSubSubscriptionAsync(eventType, version,
      new Dictionary<string, string>(conditions), TwitchLib.Api.Core.Enums.EventSubTransportMethod.Websocket,
      Client.SessionId));
  }

  public JoltEventService(ILogger<JoltEventService> _, EventSubWebsocketClient client)
  {
    Client = client ?? throw new ArgumentNullException(nameof(client));

    Client.WebsocketConnected += WebsocketConnected;
    Client.WebsocketDisconnected += WebsocketDisconnected;
    Client.WebsocketReconnected += WebsocketReconnected;

    // https://dev.twitch.tv/docs/eventsub/eventsub-subscription-types/
    EventsToSubscribe.Add(("channel.update", "2", EventCondition.Broadcaster));
    Client.ChannelUpdate += OnChannelUpdate;

    EventsToSubscribe.Add(("channel.channel_points_custom_reward_redemption.add", "1", EventCondition.Broadcaster));
    Client.ChannelPointsCustomRewardRedemptionAdd += JoltRewardDispatch.Dispatch;

    EventsToSubscribe.Add(("channel.raid", "1", EventCondition.FromBroadcaster));
    Client.ChannelRaid += OnCompleteRaid;

    EventsToSubscribe.Add(("channel.moderate", "2", EventCondition.Broadcaster, EventCondition.Moderator));
    Client.ChannelModerate += OnChannelModerate;
  }

  private async Task OnCompleteRaid(object sender, ChannelRaidArgs args)
    => await EndScreenManager.OnCompleteRaid();

  private async Task OnChannelUpdate(object sender, ChannelUpdateArgs ev)
  {
    JoltCache.UpdateOwnChannelInfo();
    await JoltRewardDispatch.Modify();
    await StreamStopper.HandleStreamUpdate(await JoltCache.GetOwnChannelInfo());
  }

  private async Task OnChannelModerate(object sender, ChannelModerateArgs ev)
  {
    if (ev.Notification.Payload.Event.Action == "raid")
      await EndScreenManager.OnCreateRaid(ev.Notification.Payload.Event);
  }

  private async Task WebsocketConnected(object sender, WebsocketConnectedArgs ev)
  {
    Logger.LogInformation("Connected.");

    if (!ev.IsRequestedReconnect)
    {
      foreach (EventSubArgs args in EventsToSubscribe)
      {
        Task _ = RegisterEventSub(args.Name, args.Version, args.Conditions);
      }
    }

    await Task.Delay(0);
  }

  private async Task WebsocketDisconnected(object sender, EventArgs ev)
  {
    Logger.LogWarning("Disconnected.");

    int time = 1;
    int count = 1;
    bool connected = false;

    do
    {
      await Task.Delay(TimeSpan.FromSeconds(time));
      Logger.LogInformation($"Reconnect attempt #{count++}...");
      time *= 2;
    } while (!(connected = await Client.ReconnectAsync()) && count < 10);

    if (!connected) Logger.LogCritical("Reconnect failed.");
  }

  private async Task WebsocketReconnected(object sender, EventArgs ev)
  {
    Logger.LogWarning("Reconnected.");
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

internal struct EventSubArgs
{
  public string Name { get; init; }
  public string Version { get; init; }
  public KeyValuePair<string, string>[] Conditions { get; init; }

  public static implicit operator EventSubArgs((string Name, string Version, KeyValuePair<string, string> Condition) input)
    => new EventSubArgs
    {
      Name = input.Name,
      Version = input.Version,
      Conditions = new KeyValuePair<string, string>[] { input.Condition }
    };

  public static implicit operator EventSubArgs(
    (string Name, string Version, KeyValuePair<string, string> Condition1, KeyValuePair<string, string> Condition2) input
  )
    => new EventSubArgs
    {
      Name = input.Name,
      Version = input.Version,
      Conditions = new KeyValuePair<string, string>[] { input.Condition1, input.Condition2 }
    };
}