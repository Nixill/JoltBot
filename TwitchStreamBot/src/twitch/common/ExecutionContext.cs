using System.Diagnostics.CodeAnalysis;
using Nixill.Streaming.JoltBot.Data;
using TwitchLib.Client.Events;
using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;
using TwitchLib.EventSub.Websockets.Core.EventArgs.Channel;

namespace Nixill.Streaming.JoltBot.Twitch;

public abstract class BaseContext
{
  public string UserName { get; protected init; }
  public string UserId { get; protected init; }
  public string UserLogin { get; protected init; }

  public string Message { get; protected init; }

  public LimitTarget LimitedAs { get; protected init; }

  public Func<string, Task> ReplyAsync { get; protected init; }
  public Func<string, Task> MessageAsync { get; protected init; }
}

public class CommandContext : BaseContext
{
  public OnChatCommandReceivedArgs ChatCommandArgs { get; init; }

  public CommandContext(OnChatCommandReceivedArgs commandArgs)
  {
    ChatCommandArgs = commandArgs;

    ReplyAsync = msg => JoltChatBot.Client.SendReplyAsync(TwitchJson.Channel.Name, commandArgs.ChatMessage.Id, msg);
    MessageAsync = msg => JoltChatBot.Client.SendMessageAsync(TwitchJson.Channel.Name, msg);

    UserName = commandArgs.ChatMessage.DisplayName;
    UserLogin = commandArgs.ChatMessage.Username;
    UserId = commandArgs.ChatMessage.UserId;

    Message = commandArgs.ChatMessage.Message;

    LimitedAs = LimitTarget.Command;
  }
}

public class RewardContext : BaseContext
{
  public ChannelPointsCustomRewardRedemption RedemptionArgs { get; init; }

  public RewardContext(ChannelPointsCustomRewardRedemptionArgs args)
  {
    RedemptionArgs = args.Notification.Payload.Event;

    UserName = RedemptionArgs.UserName;
    UserLogin = RedemptionArgs.UserLogin;
    UserId = RedemptionArgs.UserId;

    Message = RedemptionArgs.UserInput;

    LimitedAs = LimitTarget.RewardUsage;

    ReplyAsync = msg => JoltChatBot.Client.SendMessageAsync(TwitchJson.Channel.Name, $"@{UserLogin} {msg}");
    MessageAsync = msg => JoltChatBot.Client.SendMessageAsync(TwitchJson.Channel.Name, msg);
  }
}

public class StreamDeckContext : BaseContext
{
  public StreamDeckContext(string msg)
  {
    UserName = "";
    UserId = "";
    UserLogin = "";
    Message = msg;
    LimitedAs = LimitTarget.StreamDeck;
    ReplyAsync = msg => JoltChatBot.Client.SendMessageAsync(TwitchJson.Channel.Name, msg);
    MessageAsync = ReplyAsync;
  }
}

public class RewardPrecheckContext : BaseContext
{
  private RewardPrecheckContext()
  {
    UserName = "";
    UserId = "";
    UserLogin = "";
    Message = "";
    LimitedAs = LimitTarget.RewardPrecheck;
    ReplyAsync = msg => Task.CompletedTask;
    MessageAsync = ReplyAsync;
  }

  public static readonly RewardPrecheckContext Instance = new();
}

public class TickerTextCheckContext : BaseContext
{
  private TickerTextCheckContext()
  {
    UserName = "";
    UserId = "";
    UserLogin = "";
    Message = "";
    LimitedAs = LimitTarget.TickerMessage;
    ReplyAsync = msg => Task.CompletedTask;
    MessageAsync = ReplyAsync;
  }

  public static readonly TickerTextCheckContext Instance = new();
}
