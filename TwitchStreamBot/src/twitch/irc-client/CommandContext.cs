using Nixill.Streaming.JoltBot.JSON;
using TwitchLib.Client.Events;

namespace Nixill.Streaming.JoltBot.Twitch;

public class CommandContext
{
  public OnChatCommandReceivedArgs ChatCommandArgs { get; init; }
  public Func<string, Task> ReplyAsync { get; init; }
  public Func<string, Task> MessageAsync { get; init; }

  public CommandContext(OnChatCommandReceivedArgs commandArgs)
  {
    ChatCommandArgs = commandArgs;
    ReplyAsync = commandArgs.ReplyAsync;
    MessageAsync = commandArgs.MessageAsync;
  }

  public CommandContext()
  {
    ChatCommandArgs = null;
    ReplyAsync = msg => JoltChatBot.Client.SendMessageAsync(TwitchJson.Channel.Name, msg);
    MessageAsync = ReplyAsync;
  }
}