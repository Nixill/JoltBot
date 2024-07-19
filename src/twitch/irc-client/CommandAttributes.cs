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

[Flags]
// Exact values are not guaranteed to remain static. Always reference the
// constants and not exact values. Changes to these values will not be
// considered backwards-incompatible (except for removals of values).
public enum TwitchUserGroup
{
  // Users are sorted into all categories they fall into:

  // The broadcaster, i.e. the owner of the channel in which the command
  // is run.
  Broadcaster = 128,
  // Any "Editor" of the channel in which the command is run (if such
  // information is known). The Broadcaster is automatically an Editor.
  Editor = 64,
  // Any Moderator of the channel in which the command is run. The
  // Broadcaster is automatically a Moderator.
  Moderator = 32,
  // Any VIP of the channel in which the command is run. The Broadcaster
  // and any Moderator is automatically a VIP, even if the user has not
  // unlocked VIP Badges yet.
  VIP = 16,
  // Any subscriber of the channel in which the command is run. The
  // Broadcaster is automatically a Subscriber IFF the channel has
  // Affiliate or Partner.
  Subscriber = 8,
  // A regular of the channel in which the command is run (which has an
  // arbitrary and as-of-yet-undecided definition; will probably be
  // something that can be assigned or taken away within the bot).
  Regular = 4,
  // A follower of the channel (if such information is known). The
  // Broadcaster is automatically a Follower.
  Follower = 2,
  // Anyone who does not fit into an above category. 
  Anyone = 1
}

[AttributeUsage(AttributeTargets.Method)]
public class AllowedGroupsAttribute : Attribute
{
  public readonly TwitchUserGroup[] AllowList;
  public readonly TwitchUserGroup[] DenyList;

  public AllowedGroupsAttribute(params TwitchUserGroup[] groups) : this(groups.Select(g => (true, g)).ToArray()) { }
  public AllowedGroupsAttribute(params (bool Allowed, TwitchUserGroup Group)[] groups)
  {
    AllowList = groups.Where(g => g.Allowed).Select(g => g.Group).ToArray();
    DenyList = groups.Where(g => !g.Allowed).Select(g => g.Group).ToArray();
  }

  public bool CheckEditor => AllowList.Concat(DenyList).Any(x => (x & TwitchUserGroup.Editor) == TwitchUserGroup.Editor);
  public bool CheckFollower => AllowList.Concat(DenyList).Any(x => (x & TwitchUserGroup.Follower) == TwitchUserGroup.Follower);
}

public static class AllowedGroupsAttributeExtension
{
  public static bool IsAllowed(this AllowedGroupsAttribute attr, TwitchUserGroup group)
    => attr == null ||
    ((attr.AllowList.Length == 0 || attr.AllowList.Any(g => (g & group) == g))
    && !(attr.DenyList.Any(g => (g & group) == g)));

  public static bool CheckEditor(this AllowedGroupsAttribute attr)
    => attr == null || attr.CheckEditor;

  public static bool CheckFollower(this AllowedGroupsAttribute attr)
    => attr == null || attr.CheckFollower;
}

[AttributeUsage(AttributeTargets.Method)]
public class AllowAtLeastAttribute : AllowedGroupsAttribute
{
  public AllowAtLeastAttribute(TwitchUserGroup group)
    : base(Enum.GetValues<TwitchUserGroup>().Where(g => g >= group).ToArray()) { }
}

[AttributeUsage(AttributeTargets.Method)]
public class AllowAtMostAttribute : AllowedGroupsAttribute
{
  public AllowAtMostAttribute(TwitchUserGroup group)
    : base(Enum.GetValues<TwitchUserGroup>().Where(g => g <= group).ToArray()) { }
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
