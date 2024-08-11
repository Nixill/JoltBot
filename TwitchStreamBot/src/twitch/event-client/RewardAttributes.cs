using System.Reflection;
using Nixill.Streaming.JoltBot.Twitch.Api;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;

namespace Nixill.Streaming.JoltBot.Twitch.Events;

[AttributeUsage(AttributeTargets.Method)]
public class ChannelPointsRewardAttribute(string name) : Attribute
{
  public string Name { get; init; } = name;

  public static IEnumerable<MethodInfo> GetMethods(Assembly asm)
    => RewardContainerAttribute.GetTypes(asm)
      .SelectMany(GetMethods);

  public static IEnumerable<MethodInfo> GetMethods(Type t)
    => t.GetMethods()
      .Where(m => m.GetCustomAttribute(typeof(ChannelPointsRewardAttribute)) != null);
}

[AttributeUsage(AttributeTargets.Class)]
public class RewardContainerAttribute : Attribute
{
  public static IEnumerable<Type> GetTypes(Assembly asm)
    => asm.GetTypes()
      .Where(t => t.GetCustomAttribute(typeof(RewardContainerAttribute)) != null);
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public abstract class RewardModifierAttribute : Attribute
{
  public string Name = null;
  public int Price = -1;
  public string Description = null;
  public bool Enable = false;
  public bool Disable = false;

  public bool StopIfApplicable = false;

  public abstract bool IsApplicable(ChannelInformation info);
}

public class DefaultRewardModifierAttribute : RewardModifierAttribute
{
  public override bool IsApplicable(ChannelInformation info) => true;
}

public class ModifyWhenGameIsAttribute(string pattern, bool wholeMatchOnly = false,
  StringComparison comp = StringComparison.CurrentCultureIgnoreCase) : RewardModifierAttribute
{
  public string Pattern = pattern;
  public bool WholeMatchOnly = wholeMatchOnly;
  public StringComparison Comparer = comp;

  public override bool IsApplicable(ChannelInformation info)
  {
    string gameName = info.GameName;

    if (WholeMatchOnly) return gameName.Equals(Pattern, Comparer);
    else return gameName.Contains(Pattern, Comparer);
  }
}

public class ModifyWhenTitleContainsAttribute(string pattern,
  StringComparison comp = StringComparison.CurrentCultureIgnoreCase) : RewardModifierAttribute
{
  public string Pattern = pattern;
  public StringComparison Comparer = comp;

  public override bool IsApplicable(ChannelInformation info)
    => info.Title.Contains(Pattern, Comparer);
}

public class ModifyWhenTagPresentAttribute(string tag, bool wholeMatchOnly = true,
  StringComparison comp = StringComparison.CurrentCultureIgnoreCase) : RewardModifierAttribute
{
  public string Tag = tag;
  public bool WholeMatchOnly = wholeMatchOnly;
  public StringComparison Comparer = comp;

  public override bool IsApplicable(ChannelInformation info)
    => WholeMatchOnly
      ? info.Tags.Any(x => x.Equals(Tag, Comparer))
      : info.Tags.Any(x => x.Contains(Tag, Comparer));
}