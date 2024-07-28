using DSharpPlus;
using DSharpPlus.Entities;
using Nixill.Streaming.JoltBot.JSON;

namespace Nixill.Streaming.JoltBot.Discord;

public static class WebhookClient
{
  static DiscordWebhookClient Client;

  public static async Task SetUp()
  {
    Client = new();
    foreach (var value in DiscordJson.Webhooks)
    {
      await Client.AddWebhookAsync(value.Value.ID, value.Value.Secret);
    }
  }

  static DiscordWebhook GetWebhook(string channelName)
    => Client.Webhooks
      .Where(x => x.ChannelId == DiscordJson.Webhooks[channelName].ID)
      .First();

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