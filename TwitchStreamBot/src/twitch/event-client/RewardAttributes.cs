using System.Reflection;

namespace Nixill.Streaming.JoltBot.Twitch.Events;

[AttributeUsage(AttributeTargets.Method)]
public class RewardAttribute : Attribute
{
  public string Uuid { get; init; } = null;
  public string Name { get; init; } = null;

  public static IEnumerable<MethodInfo> GetMethods(Assembly asm)
    => RewardContainerAttribute.GetTypes(asm)
      .SelectMany(GetMethods);

  public static IEnumerable<MethodInfo> GetMethods(Type t)
    => t.GetMethods()
      .Where(m => m.GetCustomAttribute(typeof(RewardAttribute)) != null);
}

[AttributeUsage(AttributeTargets.Class)]
public class RewardContainerAttribute : Attribute
{
  public static IEnumerable<Type> GetTypes(Assembly asm)
    => asm.GetTypes()
      .Where(t => t.GetCustomAttribute(typeof(RewardContainerAttribute)) != null);
}
