using System.Reflection;
using TwitchLib.Client.Events;

namespace Nixill.Streaming.JoltBot.Twitch;

[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute : Attribute
{
  public readonly string Name;
  public readonly string[] Aliases;

  public CommandAttribute(string name, params string[] aliases)
  {
    Name = name;
    Aliases = aliases;
  }

  public static IEnumerable<MethodInfo> GetMethods(Assembly asm)
    => CommandContainerAttribute.GetTypes(asm)
      .SelectMany(GetMethods);

  public static IEnumerable<MethodInfo> GetMethods(Type t)
    => t.GetMethods()
      .Where(m => m.GetCustomAttribute(typeof(CommandAttribute)) != null);
}

public enum TwitchUserGroup
{
  // Users are sorted into the highest-numbered category they fall into:

  // The broadcaster, i.e. the owner of the channel in which the command
  // is run.
  Broadcaster = 10,
  // Any "Editor" of the channel in which the command is run (if such
  // information is known). If not, editors will fall back to the next
  // available group that applies to them.
  Editor = 9,
  // Any Moderator of the channel in which the command is run.
  Moderator = 8,
  // Any VIP of the channel in which the command is run.
  VIP = 7,
  // Any subscriber of the channel in which the command is run.
  Subscriber = 6,
  // A regular of the channel in which the command is run (which has an
  // arbitrary and as-of-yet-undecided definition; will probably be
  // something that can be assigned or taken away within the bot).
  Regular = 4,
  // A follower of the channel.
  Follower = 2,
  // Anyone who does not fit into an above category.
  Anyone = 0
}

[AttributeUsage(AttributeTargets.Method)]
public class AllowedGroupsAttribute : Attribute
{
  public readonly TwitchUserGroup[] List;
  public readonly bool IsAllowList;

  public AllowedGroupsAttribute(params TwitchUserGroup[] groups)
    : this(true, groups) { }
  public AllowedGroupsAttribute(bool isAllowList, params TwitchUserGroup[] groups)
  {
    List = groups.ToArray();
    IsAllowList = isAllowList;
  }

  public bool IsAllowed(TwitchUserGroup group) =>
    (List.Contains(group)) != IsAllowList;
}

[AttributeUsage(AttributeTargets.Method)]
public class AllowAtLeastAttribute : AllowedGroupsAttribute
{
  public AllowAtLeastAttribute(TwitchUserGroup group)
    : base(true, Enum.GetValues<TwitchUserGroup>().Where(g => g >= group).ToArray()) { }
}

[AttributeUsage(AttributeTargets.Method)]
public class AllowAtMostAttribute : AllowedGroupsAttribute
{
  public AllowAtMostAttribute(TwitchUserGroup group)
    : base(true, Enum.GetValues<TwitchUserGroup>().Where(g => g <= group).ToArray()) { }
}

[AttributeUsage(AttributeTargets.Class)]
public class CommandContainerAttribute : Attribute
{
  public static IEnumerable<Type> GetTypes(Assembly asm)
    => asm.GetTypes()
      .Where(t => t.GetCustomAttribute(typeof(CommandContainerAttribute)) != null);
}

[AttributeUsage(AttributeTargets.Method)]
public class DeserializerAttribute : Attribute
{
  public static IEnumerable<MethodInfo> GetMethods(Assembly asm)
    => DeserializerContainerAttribute.GetTypes(asm)
      .SelectMany(GetMethods);

  public static IEnumerable<MethodInfo> GetMethods(Type t)
    => t.GetMethods()
      .Where(m => m.GetCustomAttribute(typeof(DeserializerAttribute)) != null);
}

[AttributeUsage(AttributeTargets.Class)]
public class DeserializerContainerAttribute : Attribute
{
  public static IEnumerable<Type> GetTypes(Assembly asm)
    => asm.GetTypes()
      .Where(t => t.GetCustomAttribute(typeof(DeserializerContainerAttribute)) != null);
}

[AttributeUsage(AttributeTargets.Parameter)]
public class LongTextAttribute : Attribute { }
