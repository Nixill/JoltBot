using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Nixill.Streaming.JoltBot.JSON;
using TwitchLib.EventSub.Websockets.Core.EventArgs.Channel;

namespace Nixill.Streaming.JoltBot.Twitch.Events;

public static class JoltRewardDispatch
{
  static readonly ILogger Logger = Log.Factory.CreateLogger(typeof(JoltRewardDispatch));
  static readonly Dictionary<string, BotRedemption> Redemptions = [];

  public static void Register()
  {
    List<Exception> errors = [];
    IEnumerable<MethodInfo> rewardHandlers = RewardAttribute.GetMethods(typeof(JoltRewardDispatch).Assembly);

    foreach (MethodInfo m in rewardHandlers)
    {
      try
      {
        var pars = m.GetParameters();
        if (!m.IsStatic) throw new IllegalRewardException(m, "It must be a static method.");
        if (pars.Length != 1) throw new IllegalRewardException(m, "It must have exactly one parameter.");
        if (m.ReturnType != typeof(Task)) throw new IllegalRewardException(m, "It must return Task.");
        if (!pars[0].ParameterType.IsAssignableFrom(typeof(RewardContext)))
          throw new IllegalCommandException(m, "The sole parameter must be Redemption Context (or a less derived type).");

        RewardAttribute attr = m.GetCustomAttribute<RewardAttribute>();

        // TODO automatic registration
        string rewardUuid = attr.Uuid ?? TwitchJson.RewardKeys.GetValueOrDefault(attr.Name ?? "")
          ?? throw new IllegalRewardException(m, $"Reward {attr.Name ?? "null"} not found in twitch.json");

        BotRedemption rdmp = new BotRedemption
        {
          Uuid = rewardUuid,
          Method = m,
          Restrictions = m.GetCustomAttributes<LimitAttribute>()
        };

        Redemptions[rewardUuid] = rdmp;
      }
      catch (IllegalRewardException ex)
      {
        Logger.LogError(ex, "Could not register channel points reward");
      }
    }
  }

  internal static async Task Dispatch(object sender, ChannelPointsCustomRewardRedemptionArgs args)
  {
    RewardContext ctx = new RewardContext(args);

  }
}

public class BotRedemption
{
  public string Uuid { get; init; }
  public MethodInfo Method { get; init; }
  public IEnumerable<LimitAttribute> Restrictions { get; init; }
}