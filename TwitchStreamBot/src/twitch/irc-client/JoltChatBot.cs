using Microsoft.Extensions.Logging;
using Nixill.Streaming.JoltBot.Data;
using Nixill.Streaming.JoltBot.OBS;
using NReco.Logging.File;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

namespace Nixill.Streaming.JoltBot.Twitch;

public static class JoltChatBot
{
  internal static TwitchClient Client;

  static ILogger Logger = Log.Factory.CreateLogger(typeof(JoltChatBot));

  public static async Task SetUp(AuthInfo auth, string channelName)
  {
    ConnectionCredentials credentials = new ConnectionCredentials(auth.Name, auth.Token);

    CommandDispatch.Register();

    Client = new TwitchClient(loggerFactory: Log.Factory)
    {
      ChatCommandIdentifiers = { '!' }
    };

    Client.OnConnected += ConnectHandler;
    Client.OnJoinedChannel += JoinHandler;
    Client.OnFailureToReceiveJoinConfirmation += FailJoinHandler;
    Client.OnSendReceiveData += DataHandler;
    Client.OnChatCommandReceived += CommandDispatch.Dispatch;

    Client.OnUnraidNotification += RaidEnded;

    Logger.LogInformation("Attempting to connect...");
    Client.Initialize(credentials, channelName);
    await Client.ConnectAsync();
  }

  private static async Task RaidEnded(object sender, OnUnraidNotificationArgs e)
  {
    await EndScreenManager.OnCancelRaid(e);
  }

  public static Task ConnectHandler(object sender, OnConnectedEventArgs ev)
  {
    Logger.LogInformation("Connection established.");
    return Task.CompletedTask;
  }

  public static async Task JoinHandler(object sender, OnJoinedChannelArgs ev)
  {
    Logger.LogInformation($"Jolt chatbot connected to {ev.Channel}.");
    await Client.SendMessageAsync(ev.Channel, "Jolt chatbot connected.");
  }

  public static Task FailJoinHandler(object sender, OnFailureToReceiveJoinConfirmationArgs ev)
  {
    Logger.LogError($"Failed to join {ev.Exception.Channel}: {ev.Exception.Details ?? "No further details."}");
    return Task.CompletedTask;
  }

  public static Task DataHandler(object sender, OnSendReceiveDataArgs ev)
  {
    Logger.LogTrace($"{ev.Direction} {ev.Data}");
    return Task.CompletedTask;
  }

  public static async Task Chat(string message) =>
    await Client.SendMessageAsync(TwitchJson.Channel.Name, message);
}