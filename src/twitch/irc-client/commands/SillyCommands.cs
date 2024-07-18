using TwitchLib.Client.Events;

namespace Nixill.Streaming.JoltBot.Twitch.Commands;

[CommandContainer]
public static class SillyCommands
{
  [Command("ban")]
  public static async Task BanCommand(OnChatCommandReceivedArgs ev, [LongText] string name)
  {
    if (ev.GetUserGroup() < TwitchUserGroup.Moderator && Random.Shared.NextDouble() < 0.05)
    {
      name = ev.ChatMessage.DisplayName;
    }

    await ev.ReplyAsync($"You're banned, {name}!");
  }

  [Command("coinflip", "coin")]
  public static async Task CoinFlipCommand(OnChatCommandReceivedArgs ev)
  {
    await ev.ReplyAsync("Flipping a coin...");
    await Task.Delay(1000);
    await ev.MessageAsync($"It landed {(Random.Shared.Next(2) == 1 ? "heads" : "tails")}.");
  }

  [Command("countdown")]
  public static async Task CountdownCommand(OnChatCommandReceivedArgs ev)
  {
    await ev.ReplyAsync("Ready?");
    await Task.Delay(1000);
    await ev.MessageAsync("5...");
    await Task.Delay(1000);
    await ev.MessageAsync("4...");
    await Task.Delay(1000);
    await ev.MessageAsync("3...");
    await Task.Delay(1000);
    await ev.MessageAsync("2...");
    await Task.Delay(1000);
    await ev.MessageAsync("1...");
    await Task.Delay(1000);
    await ev.MessageAsync("Go!");
  }
}