using System.Reflection;

namespace Nixill.Streaming.JoltBot.Twitch.Events;

public class IllegalRewardException : Exception
{
  public readonly MethodInfo Method;

  public IllegalRewardException(MethodInfo m, string message) : base($"The method {m} is an invalid command: {message}")
  {
    Method = m;
  }
}
