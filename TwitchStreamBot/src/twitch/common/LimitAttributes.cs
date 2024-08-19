using Nixill.Streaming.JoltBot.Twitch.Api;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;

namespace Nixill.Streaming.JoltBot.Twitch;

[AttributeUsage(AttributeTargets.Method
  | AttributeTargets.Property
  | AttributeTargets.Field,
  AllowMultiple = true)]
public abstract class LimitAttribute : Attribute
{
  public LimitTarget AppliesTo { get; init; } = LimitTarget.All;
  public string FailWarning { get; init; } = null;
  public bool Invert { get; init; } = false;
  public bool StopOnDeny { get; init; } = true;
  public bool StopOnAllow { get; init; } = false;

  public async Task<bool?> PassesCondition(BaseContext ctx, ChannelInformation info)
  {
    if (!AppliesTo.HasFlag(ctx.LimitedAs)) return null;
    return await ConditionCheck(ctx, info) != Invert;
  }

  protected abstract Task<bool> ConditionCheck(BaseContext ctx, ChannelInformation info);
}

[Flags]
public enum LimitTarget
{
  Command = 1,
  RewardUsage = 2,
  RewardPrecheck = 4,
  StreamDeck = 8,
  TickerMessage = 16,

  All = Command | RewardUsage | RewardPrecheck | StreamDeck | TickerMessage,
  Common = Command | RewardUsage | RewardPrecheck | TickerMessage,
  HasUser = Command | RewardUsage
}

public class AllowedWithGameAttribute(string pattern, bool wholeMatchOnly = false,
  StringComparison comp = StringComparison.CurrentCultureIgnoreCase) : LimitAttribute
{
  public string Pattern = pattern;
  public bool WholeMatchOnly = wholeMatchOnly;
  public StringComparison Comparer = comp;

  protected override Task<bool> ConditionCheck(BaseContext ctx, ChannelInformation info)
  {
    string gameName = info.GameName;

    if (WholeMatchOnly) return Task.FromResult(gameName.Equals(Pattern, Comparer));
    else return Task.FromResult(gameName.Contains(Pattern, Comparer));
  }
}

public class AllowedWithTitleAttribute(string pattern,
  StringComparison comp = StringComparison.CurrentCultureIgnoreCase) : LimitAttribute
{
  public string Pattern = pattern;
  public StringComparison Comparer = comp;

  protected override Task<bool> ConditionCheck(BaseContext ctx, ChannelInformation info)
    => Task.FromResult(info.Title.Contains(Pattern, Comparer));
}

public class AllowedWithTagAttribute(string tag, bool wholeMatchOnly = true,
  StringComparison comp = StringComparison.CurrentCultureIgnoreCase) : LimitAttribute
{
  public string Tag = tag;
  public bool WholeMatchOnly = wholeMatchOnly;
  public StringComparison Comparer = comp;

  protected override Task<bool> ConditionCheck(BaseContext ctx, ChannelInformation info)
    => WholeMatchOnly
      ? Task.FromResult(info.Tags.Any(x => x.Equals(Tag, Comparer)))
      : Task.FromResult(info.Tags.Any(x => x.Contains(Tag, Comparer)));
}

public class AllowedUserGroupsAttribute : LimitAttribute
{
  public readonly TwitchUserGroup[] Groups;

  public AllowedUserGroupsAttribute(params TwitchUserGroup[] groups)
  {
    Groups = groups.Distinct().ToArray();
    AppliesTo = LimitTarget.HasUser;
  }

  protected override async Task<bool> ConditionCheck(BaseContext ctx, ChannelInformation info)
  {
    TwitchUserGroup group = await JoltCache.GetUserGroup(ctx.UserId);

    return Groups.Any(g => (group & g) == g);
  }
}

public class ChanceToAppear : LimitAttribute
{
  public double Chance;

  public ChanceToAppear(double chance)
  {
    Chance = chance;
    AppliesTo = LimitTarget.TickerMessage;
  }

  protected override Task<bool> ConditionCheck(BaseContext ctx, ChannelInformation info)
  {
    return Task.FromResult(Random.Shared.NextDouble() < Chance);
  }
}

public class DisableTemporarily : LimitAttribute
{
  public DisableTemporarily()
  {
    StopOnDeny = true;
  }

  protected override Task<bool> ConditionCheck(BaseContext ctx, ChannelInformation info)
    => Task.FromResult(false);
}