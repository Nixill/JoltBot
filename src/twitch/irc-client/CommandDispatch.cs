using System.Reflection;
using Microsoft.Extensions.Logging;
using Nixill.Utils;
using TwitchLib.Api;
using TwitchLib.Client.Events;

namespace Nixill.Streaming.JoltBot.Twitch;

public static class CommandDispatch
{
  static ILogger logger = Log.Factory.CreateLogger("CommandDispatch");
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
        if (!pars[0].ParameterType.IsAssignableTo(typeof(OnChatCommandReceivedArgs)))
          throw new IllegalCommandException(m, "The first parameter must be OnChatCommandReceivedArgs.");
        var unusableTypes = pars.Skip(1).Select(p =>
        {
          if (p.CustomAttributes.Any(a => a.AttributeType == typeof(ParamArrayAttribute)))
            return p.ParameterType.GetElementType();
          else
            return p.ParameterType;
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
            Type = isVararg ? p.ParameterType.GetElementType() : p.ParameterType,
            Optional = p.HasDefaultValue
          });
        }

        TwitchUserGroup[] groups;

        var groupsAttr = m.GetCustomAttribute<AllowedGroupsAttribute>();

        if (groupsAttr == null)
          groups = Enum.GetValues<TwitchUserGroup>().ToArray();
        else if (groupsAttr.IsAllowList)
          groups = groupsAttr.List;
        else
          groups = Enum.GetValues<TwitchUserGroup>().Except(groupsAttr.List).ToArray();

        BotCommand cmd = new BotCommand
        {
          Name = attr.Name,
          Aliases = attr.Aliases,
          Method = m,
          AllowedGroups = groups,
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
    string commandName = "";

    while (true)
    {
      commandName += (commandName.Length == 0 ? "" : " ") + words.Pop().ToLower();

      if (Commands.ContainsKey(commandName)) break;
      if (words.Count == 0) return; // No error if no such command
    }

    BotCommand cmd = Commands[commandName];

    if (!cmd.AllowedGroups.Contains(ev.GetUserGroup()))
    {
      await ev.ReplyAsync("You are not allowed to use this command!");
      return;
    }

    List<object> pars = new() { ev };

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
        else if (!par.Optional)
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
      await ev.ReplyAsync(usage);
    }
    catch (TargetInvocationException e) when (e.InnerException is NoValueException)
    {
      string usage = $"Usage: !{commandName}" + cmd.Parameters
        .Select(p => p.IsVararg ? $" [{p.Name} ...]" : p.Optional ? $" [{p.Name}]" : $" <{p.Name}>")
        .SJoin("");
      await ev.ReplyAsync(usage);
    }
    catch (TargetInvocationException e) when (e.InnerException is InvalidDeserializeException ide)
    {
      string error = $"Error: {ide.Value} is not a valid {ide.HumanReadableType}"
        + ((ide.CustomMessage != null) ? $" {ide.CustomMessage}." : ".");
      await ev.ReplyAsync(error);
    }
    catch (TargetInvocationException e)
    {
      await ev.ReplyAsync($"Error: {e.InnerException.GetType().Name}: {e.InnerException.Message}");
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
  public TwitchUserGroup[] AllowedGroups { get; init; }
  public MethodInfo Method { get; init; }
}

public class BotParam
{
  public string Name { get; init; }
  public Type Type { get; init; }
  public bool IsLongText { get; init; }
  public bool IsVararg { get; init; }
  public bool Optional { get; init; }

  public object Deserialize(IList<string> input) => CommandDispatch.Deserialize(Type, input, IsLongText);
}

public static class CommandExtensions
{
  public static Task ReplyAsync(this OnChatCommandReceivedArgs ev, string message)
    => TwitchBot.Client.SendReplyAsync(ev.ChatMessage.Channel, ev.ChatMessage.Id, message);

  public static Task MessageAsync(this OnChatCommandReceivedArgs ev, string message)
    => TwitchBot.Client.SendMessageAsync(ev.ChatMessage.Channel, message);

  public static TwitchUserGroup GetUserGroup(this OnChatCommandReceivedArgs ev)
  {
    var msg = ev.ChatMessage;
    var detail = msg.UserDetail;
    if (msg.Channel == msg.Username) return TwitchUserGroup.Broadcaster;
    // if (TwitchAPI.Helix.Channels.GetChannelEditorsAsync().Contains(user)) return TwitchUserGroup.Editor;
    if (detail.IsModerator) return TwitchUserGroup.Moderator;
    if (detail.IsVip) return TwitchUserGroup.VIP;
    if (detail.IsSubscriber) return TwitchUserGroup.Subscriber;
    // TODO add a way to get a regular
    // if (new TwitchAPI().Helix.Channels.GetChannelFollowersAsync(channel, user)) return TwitchUserGroup.Follower;
    return TwitchUserGroup.Anyone;
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
