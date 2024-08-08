namespace Nixill.Streaming.JoltBot.Twitch;

public abstract class LimitAttribute : Attribute
{
  public LimitTarget AppliesTo { get; init; } = LimitTarget.All;

  public ConditionCheckResult PassesCondition(BaseContext ctx)
  {
    if (!ctx.LimitedAs.HasFlag(AppliesTo)) return true;
    return ConditionCheck(ctx);
  }

  protected abstract ConditionCheckResult ConditionCheck(BaseContext ctx);
}

public readonly struct ConditionCheckResult
{
  public bool ConditionPassed { get; init; }
  public bool WarnOfFail { get; init; } = true;

  public ConditionCheckResult() { }

  public static implicit operator ConditionCheckResult(bool input)
    => new ConditionCheckResult { ConditionPassed = input };
}

[Flags]
public enum LimitTarget
{
  Command = 1,
  Reward = 2,
  StreamDeck = 4,

  All = Command | Reward | StreamDeck
}

// [AttributeUsage(AttributeTargets.Method
//   | AttributeTargets.Property
//   | AttributeTargets.Field,
//   AllowMultiple = true)]
// public class ValidDuringGameAttribute : LimitAttribute
// {

// }