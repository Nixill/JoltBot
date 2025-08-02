using System.Reflection;
using Microsoft.Extensions.Logging;
using Nixill.Streaming.JoltBot.Data;
using Nixill.Streaming.JoltBot.Twitch.Api;
using Nixill.Utils.Extensions;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Channels.GetChannelEditors;
using TwitchLib.Api.Helix.Models.Channels.GetChannelFollowers;
using TwitchLib.Client.Events;

namespace Nixill.Streaming.JoltBot.Twitch;

public static class CommandDispatch
{
  static readonly ILogger logger = Log.Factory.CreateLogger(typeof(CommandDispatch));
  static Dictionary<string, BotCommand> Commands;
  static int LongestCommandName;
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
        if (!pars[0].ParameterType.IsAssignableFrom(typeof(CommandContext)))
          throw new IllegalCommandException(m, "The first parameter must be CommandContext (or a less derived type).");
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
          throw new IllegalCommandException(m, $"No deserializer exists for type(s) {unusableTypes.StringJoin(", ")}");
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
            DefaultValue = p.HasDefaultValue ? p.DefaultValue : null
          });
        }

        BotCommand cmd = new BotCommand
        {
          Name = attr.Name,
          Aliases = attr.Aliases,
          Method = m,
          Restrictions = m.GetCustomAttributes<LimitAttribute>(),
          Parameters = [.. botParams]
        };

        LongestCommandName = Math.Max(LongestCommandName,
          attr.Aliases.Prepend(attr.Name).Max(x => x.Count(c => c == ' ') + 1));

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
    string message = words.StringJoin(" ");
    string commandName = "";
    List<string> commandNameWords = words[0..Math.Min(words.Count, LongestCommandName)];
    words = words[Math.Min(words.Count, LongestCommandName)..];

    while (true)
    {
      commandName = commandNameWords.StringJoin(" ");

      if (Commands.ContainsKey(commandName)) break;

      string word = commandNameWords[^1];
      commandNameWords.RemoveAt(commandNameWords.Count - 1);
      words.Insert(0, word);

      if (commandNameWords.Count == 0) return; // No error if no such command
    }

    BotCommand cmd = Commands[commandName];
    BaseContext ctx = (ev != null) ? new CommandContext(ev) : new StreamDeckContext(message);
    bool allowed = true;
    string failMessage = null;

    foreach (var limit in cmd.Restrictions)
    {
      bool? result = await limit.PassesCondition(ctx, await JoltCache.GetOwnChannelInfo());
      if (result == true)
      {
        allowed = true;
        failMessage = null;
        if (limit.StopOnAllow) break;
      }
      else if (result == false)
      {
        allowed = false;
        failMessage = limit.FailWarning;
        if (limit.StopOnDeny) break;
      }
    }

    if (!allowed)
    {
      if (failMessage != null) await ctx.ReplyAsync($"Execution failed: {failMessage}");
      return;
    }

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
        .StringJoin("");
      await ctx.ReplyAsync(usage);
    }
    catch (TargetInvocationException e) when (e.InnerException is NoValueException)
    {
      string usage = $"Usage: !{commandName}" + cmd.Parameters
        .Select(p => p.IsVararg ? $" [{p.Name} ...]" : p.Optional ? $" [{p.Name}]" : $" <{p.Name}>")
        .StringJoin("");
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
    catch (Exception e)
    {
      await ctx.ReplyAsync($"Error: {e.GetType().Name}: {e.Message}");
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
  public IEnumerable<LimitAttribute> Restrictions { get; init; }
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
