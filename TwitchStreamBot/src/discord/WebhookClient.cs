using DSharpPlus;
using DSharpPlus.Entities;
using Nixill.Streaming.JoltBot.JSON;

namespace Nixill.Streaming.JoltBot.Discord;

public static class WebhookClient
{
  static DiscordWebhookClient Client;
  static Dictionary<string, DiscordWebhook> WebhookChannels = new();

  public static async Task SetUp()
  {
    Client = new();
    foreach (var kvp in DiscordJson.Webhooks)
    {
      WebhookChannels[kvp.Key] = await Client.AddWebhookAsync(kvp.Value.ID, kvp.Value.Secret);
    }
  }

  static DiscordWebhook GetWebhook(string channelName)
    => WebhookChannels[channelName];

  public static async Task SendMessage(string channelName, string text)
    => await GetWebhook(channelName)
      .ExecuteAsync(new DiscordWebhookBuilder
      {
        Content = text
      });

  public static async Task SendPingingMessage(string channelName, string text)
    => await GetWebhook(channelName)
      .ExecuteAsync(new DiscordWebhookBuilder
      {
        Content = $"<@{DiscordJson.OwnerID}> {text}"
      }.AddMention(new UserMention(DiscordJson.OwnerID)));

  public static async Task SendFile(string channelName, string filePath)
    => await GetWebhook(channelName)
      .ExecuteAsync(new DiscordWebhookBuilder
      {
        Content = Path.GetFileName(filePath)
      }.AddFile(Path.GetFileName(filePath), File.OpenRead(filePath)));
}