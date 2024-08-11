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
  Broadcaster = 256,
  // Any "Editor" of the channel in which the command is run (if such
  // information is known). The Broadcaster is automatically an Editor.
  Editor = 128,
  // Any Moderator of the channel in which the command is run. The
  // Broadcaster is automatically a Moderator.
  Moderator = 64,
  // Any VIP of the channel in which the command is run. The Broadcaster
  // and any Moderator is automatically a VIP, even if the user has not
  // unlocked VIP Badges yet.
  VIP = 32,
  // Any subscriber of the channel in which the command is run. The
  // Broadcaster is automatically a Tier 3 Subscriber IFF the channel has
  // Affiliate or Partner.
  Tier3Subscriber = 16,
  Tier2Subscriber = 8,
  Tier1Subscriber = 4,
  // A regular of the channel in which the command is run (which has an
  // arbitrary and as-of-yet-undecided definition; will probably be
  // something that can be assigned or taken away within the bot).
  Regular = 2,
  // Anyone, whether or not they fit into an above category.
  Anyone = 1,
  // Some convenience values for ASSIGNING ROLES TO USERS.
  Tier2AndBelow = Tier1Subscriber | Tier2Subscriber,
  Tier3AndBelow = Tier2AndBelow | Tier3Subscriber,
  AllBroadcasterRoles = Broadcaster | Editor | Moderator | VIP | Tier3AndBelow,
  AllModeratorRoles = Moderator | VIP
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
