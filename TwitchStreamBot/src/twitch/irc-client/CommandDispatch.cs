using System.Reflection;
using Microsoft.Extensions.Logging;
using Nixill.Streaming.JoltBot.JSON;
using Nixill.Streaming.JoltBot.Twitch.Api;
using Nixill.Utils;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Channels.GetChannelEditors;
using TwitchLib.Api.Helix.Models.Channels.GetChannelFollowers;
using TwitchLib.Client.Events;

namespace Nixill.Streaming.JoltBot.Twitch;

public static class CommandDispatch
{
  static readonly ILogger logger = Log.Factory.CreateLogger(typeof(CommandDispatch));
  static Dictionary<string, BotCommand> Commands;
  static Dictionary<Type, Func<IList<string>, bool, object>> Deserializers;

  public static void Register()
  {
    Deserializers = new();
    IEnumerable<MethodInfo> deserializers = DeserializerAttribute.GetMethods(typeof(CommandDispatch).Assembly);

    List<Exception> errors = new();

    foreach (MethodInfo m in deserializers)
    {
      try
      {
        var pars = m.GetParameters();
        if (!m.IsStatic) throw new IllegalDeserializerException(m, "It must be a static method.");
        if (pars.Length != 2) throw new IllegalDeserializerException(m, "It must have exactly two parameters.");
        if (pars[0].ParameterType != typeof(IList<string>))
          throw new IllegalDeserializerException(m, "Its first parameter must be an IList<string>.");
        if (pars[1].ParameterType != typeof(bool))
          throw new IllegalDeserializerException(m, "Its second parameter must be a bool.");
        if (Deserializers.ContainsKey(m.ReturnType))
          throw new IllegalDeserializerException(m, "There is already another deserializer of that type.");

        Deserializers.Add(m.ReturnType, (s, b) => m.Invoke(null, new object[] { s, b }));
      }
      catch (IllegalDeserializerException ex)
      {
        logger.LogError(ex, "Could not register deserializer");
      }
    }

    Commands = new();
    IEnumerable<MethodInfo> commands = CommandAttribute.GetMethods(typeof(CommandDispatch).Assembly);

    foreach (MethodInfo m in commands)
    {
      try
      {
        var pars = m.GetParameters();
        if (!m.IsStatic) throw new IllegalCommandException(m, "It must be a static method.");
        if (pars.Length == 0) throw new IllegalCommandException(m, "It must have at least one parameter.");
        if (m.ReturnType != typeof(Task)) throw new IllegalCommandException(m, "It must return Task.");
        if (!pars[0].ParameterType.IsAssignableTo(typeof(CommandContext)))
          throw new IllegalCommandException(m, "The first parameter must be CommandContext.");
        var unusableTypes = pars.Skip(1).Select(p =>
        {
          if (p.CustomAttributes.Any(a => a.AttributeType == typeof(ParamArrayAttribute)))
            return p.ParameterType.GetElementType();
          else
          {
            Type t = Nullable.GetUnderlyingType(p.ParameterType);
            if (t != null)
              return t;
            else
              return p.ParameterType;
          }
        }).Where(t => !Deserializers.ContainsKey(t));
        if (unusableTypes.Any())
          throw new IllegalCommandException(m, $"No deserializer exists for type(s) {unusableTypes.SJoin(", ")}");
        if (pars.SkipLast(pars.Length == 1 ? 0 : 1).Any(p => p.GetCustomAttribute(typeof(LongTextAttribute)) != null))
          if (pars.Length == 1)
            throw new IllegalCommandException(m, "Only a command parameter (not the initial OnCommandReceivedArgs) may be [LongText].");
          else
            throw new IllegalCommandException(m, "Only the final parameter may be [LongText].");

        CommandAttribute attr = (CommandAttribute)m.GetCustomAttribute(typeof(CommandAttribute));

        List<BotParam> botParams = new();
        foreach (ParameterInfo p in pars.Skip(1))
        {
          bool isVararg = p.CustomAttributes.Any(a => a.AttributeType == typeof(ParamArrayAttribute));
          botParams.Add(new BotParam
          {
            Name = p.Name,
            IsLongText = p.GetCustomAttribute(typeof(LongTextAttribute)) != null,
            IsVararg = isVararg,
            Type = isVararg ? p.ParameterType.GetElementType() : Nullable.GetUnderlyingType(p.ParameterType) ?? p.ParameterType,
            Optional = p.HasDefaultValue || Nullable.GetUnderlyingType(p.ParameterType) != null,
            DefaultValue = (p.HasDefaultValue) ? p.DefaultValue : null
          });
        }

        BotCommand cmd = new BotCommand
        {
          Name = attr.Name,
          Aliases = attr.Aliases,
          Method = m,
          Access = m.GetCustomAttribute<AllowedGroupsAttribute>(),
          Parameters = botParams.ToArray()
        };

        Commands[attr.Name] = cmd;

        foreach (string alias in attr.Aliases)
        {
          Commands.TryAdd(alias, cmd);
        }
      }
      catch (IllegalCommandException ex)
      {
        logger.LogError(ex, "Could not register command");
      }
    }
  }

  public static async Task Dispatch(object sender, OnChatCommandReceivedArgs ev)
  {
    List<string> words = ev.Command.ArgumentsAsList.Prepend(ev.Command.Name).ToList();
    await Dispatch(words, ev);
  }

  public static async Task Dispatch(List<string> words, OnChatCommandReceivedArgs ev = null)
  {
    string commandName = "";

    while (true)
    {
      commandName += (commandName.Length == 0 ? "" : " ") + words.Pop().ToLower();

      if (Commands.ContainsKey(commandName)) break;
      if (words.Count == 0) return; // No error if no such command
    }

    BotCommand cmd = Commands[commandName];

    if (ev != null && cmd.Access != null && !cmd.Access.IsAllowed(await ev.GetUserGroup(checkFollower: cmd.Access.CheckFollower, checkEditor: cmd.Access.CheckEditor)))
    {
      await ev.ReplyAsync("You are not allowed to use this command!");
      return;
    }

    CommandContext ctx = (ev != null) ? new CommandContext(ev) : new CommandContext();
    List<object> pars = [ctx];

    try
    {
      foreach (BotParam par in cmd.Parameters)
      {
        if (par.IsVararg)
        {
          List<object> paramsObject = new();
          try
          {
            while (words.Count > 0)
            {
              object input = par.Deserialize(words);
              paramsObject.Add(input);
            }
          }
          catch (NoValueException) { }
          pars.Add(ArrayDeserializer.CastArrayToType(paramsObject.ToArray(), par.Type));
        }
        else if (words.Count > 0)
        {
          object input = par.Deserialize(words);
          pars.Add(input);
        }
        else if (par.Optional)
        {
          pars.Add(par.DefaultValue);
        }
        else
        {
          throw new NoValueException(par.Name);
        }
      }

      await (Task)cmd.Method.Invoke(null, pars.ToArray());
    }
    catch (NoValueException)
    {
      string usage = $"Usage: !{commandName}" + cmd.Parameters
        .Select(p => p.IsVararg ? $" [{p.Name} ...]" : p.Optional ? $" [{p.Name}]" : $" <{p.Name}>")
        .SJoin("");
      await ctx.ReplyAsync(usage);
    }
    catch (TargetInvocationException e) when (e.InnerException is NoValueException)
    {
      string usage = $"Usage: !{commandName}" + cmd.Parameters
        .Select(p => p.IsVararg ? $" [{p.Name} ...]" : p.Optional ? $" [{p.Name}]" : $" <{p.Name}>")
        .SJoin("");
      await ctx.ReplyAsync(usage);
    }
    catch (TargetInvocationException e) when (e.InnerException is InvalidDeserializeException ide)
    {
      string error = $"Error: {ide.Value} is not a valid {ide.HumanReadableType}"
        + ((ide.CustomMessage != null) ? $" {ide.CustomMessage}." : ".");
      await ctx.ReplyAsync(error);
    }
    catch (TargetInvocationException e)
    {
      await ctx.ReplyAsync($"Error: {e.InnerException.GetType().Name}: {e.InnerException.Message}");
      logger.LogError(e, "Error in command execution");
    }
  }

  public static object Deserialize(Type type, IList<string> inputs, bool isLongText)
  {
    if (Deserializers.TryGetValue(type, out var deserializer))
    {
      return deserializer(inputs, isLongText);
    }
    throw new NoDeserializerException(type);
  }
}

public class BotCommand
{
  public string Name { get; init; }
  public string[] Aliases { get; init; }
  public BotParam[] Parameters { get; init; }
  public AllowedGroupsAttribute Access { get; init; }
  public MethodInfo Method { get; init; }
}

public class BotParam
{
  public string Name { get; init; }
  public Type Type { get; init; }
  public bool IsLongText { get; init; }
  public bool IsVararg { get; init; }
  public bool Optional { get; init; }
  public object DefaultValue { get; init; }

  public object Deserialize(IList<string> input) => CommandDispatch.Deserialize(Type, input, IsLongText);
}

public static class CommandExtensions
{
  public static Task ReplyAsync(this OnChatCommandReceivedArgs ev, string message)
    => JoltChatBot.Client.SendReplyAsync(ev.ChatMessage.Channel, ev.ChatMessage.Id, message);

  public static Task MessageAsync(this OnChatCommandReceivedArgs ev, string message)
    => JoltChatBot.Client.SendMessageAsync(ev.ChatMessage.Channel, message);

  public static async Task<TwitchUserGroup> GetUserGroup(this OnChatCommandReceivedArgs ev, bool checkEditor = false, bool checkFollower = false)
  {
    await Task.Delay(0);

    TwitchUserGroup ret = 0;

    var msg = ev.ChatMessage;
    var detail = msg.UserDetail;

    Task<GetChannelEditorsResponse> getEditors = null;
    Task<GetChannelFollowersResponse> getFollowers = null;

    if (checkEditor) getEditors = JoltApiClient.WithToken((api, id) => api.Helix.Channels.GetChannelEditorsAsync(id));
    if (checkFollower) getFollowers = JoltApiClient.WithToken((api, id) => api.Helix.Channels.GetChannelFollowersAsync(id, msg.UserId));

    bool isBroadcaster = (msg.Channel == msg.Username);

    if (msg.Channel == msg.Username) ret |= TwitchUserGroup.Broadcaster;
    if (checkEditor && (isBroadcaster || (await getEditors).Data.Any(x => x.UserId == msg.UserId))) ret |= TwitchUserGroup.Editor;
    if (isBroadcaster || detail.IsModerator) ret |= TwitchUserGroup.Moderator;
    if (isBroadcaster || detail.IsModerator || detail.IsVip) ret |= TwitchUserGroup.VIP;
    if (detail.IsSubscriber) ret |= TwitchUserGroup.Subscriber;
    // TODO add a way to get a regular
    if (checkFollower && (isBroadcaster || (await getFollowers).Data.Any(x => x.UserId == msg.UserId))) ret |= TwitchUserGroup.Follower;
    if (ret == 0) ret = TwitchUserGroup.Anyone;

    return ret;
  }
}

public static class ArrayDeserializer
{
  public static object CastArrayToType(object[] array, Type type)
  {
    MethodInfo getMethod = typeof(ArrayDeserializer).GetMethod("GetArrayInType");
    MethodInfo conMethod = getMethod.MakeGenericMethod(type);
    return conMethod.Invoke(null, new object[] { array });
  }

  public static T[] GetArrayInType<T>(object[] array)
  {
    return array.Cast<T>().ToArray();
  }
}
