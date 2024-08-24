using System.Reflection;
using Microsoft.Extensions.Logging;
using Nixill.Streaming.JoltBot.Data;
using Nixill.Streaming.JoltBot.Twitch.Api;
using TwitchLib.Api.Core.Enums;
using TwitchLib.Api.Helix.Models.ChannelPoints.CreateCustomReward;
using TwitchLib.Api.Helix.Models.ChannelPoints.UpdateCustomReward;
using TwitchLib.Api.Helix.Models.ChannelPoints.UpdateCustomRewardRedemptionStatus;
using TwitchLib.EventSub.Websockets.Core.EventArgs.Channel;

namespace Nixill.Streaming.JoltBot.Twitch.Events;

public static class JoltRewardDispatch
{
  static readonly ILogger Logger = Log.Factory.CreateLogger(typeof(JoltRewardDispatch));
  static readonly Dictionary<string, BotRedemption> Redemptions = [];

  public static async Task Register()
  {
    List<Exception> errors = [];
    IEnumerable<MethodInfo> rewardHandlers = ChannelPointsRewardAttribute.GetMethods(typeof(JoltRewardDispatch).Assembly);

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

        ChannelPointsRewardAttribute attr = m.GetCustomAttribute<ChannelPointsRewardAttribute>();

        // let's do automatic registration right now actually
        string rewardUuid = TwitchJson.RewardKeys.GetValueOrDefault(attr.Name ?? "")
          ?? await CreateNewReward(attr.Name);

        BotRedemption rdmp = new BotRedemption
        {
          Uuid = rewardUuid,
          Method = m,
          Restrictions = m.GetCustomAttributes<LimitAttribute>()
            .Where(l => l.AppliesTo.HasFlag(LimitTarget.RewardUsage)),
          Modifiers = m.GetCustomAttributes<RewardModifierAttribute>()
        };

        Redemptions[rewardUuid] = rdmp;
      }
      catch (IllegalRewardException ex)
      {
        Logger.LogError(ex, "Could not register channel points reward");
      }
    }
  }

  public static async Task Modify()
  {
    var channelInfo = await JoltCache.GetOwnChannelInfo();

    foreach (var redeem in Redemptions.Values.Where(r => r.Modifiers.Any() || r.Restrictions.Any()))
    {
      string name = null;
      int? price = null;
      string topic = null;
      bool? enable = null;

      foreach (var modifier in redeem.Modifiers)
      {
        if (modifier.IsApplicable(channelInfo))
        {
          name = modifier.Name ?? name;
          price = (modifier.Price < 0) ? price : modifier.Price;
          topic = modifier.Description ?? topic;
          enable = modifier.Enable == modifier.Disable ? enable : modifier.Enable;

          if (modifier.StopIfApplicable) break;
        }
      }

      foreach (var restriction in redeem.Restrictions)
      {
        bool? passResult = await restriction.PassesCondition(RewardPrecheckContext.Instance, channelInfo);
        if (passResult == true)
        {
          enable = true;
          if (restriction.StopOnAllow) break;
        }
        else if (passResult == false)
        {
          enable = false;
          if (restriction.StopOnDeny) break;
        }
      }

      UpdateCustomRewardRequest req = null;

      if (name != null) (req ??= new()).Title = name;
      if (price != null) (req ??= new()).Cost = price;
      if (topic != null) (req ??= new()).Prompt = topic;
      if (enable != null) (req ??= new()).IsEnabled = enable;

      if (req != null)
        await JoltApiClient.WithToken((api, id) => api.Helix.ChannelPoints.UpdateCustomRewardAsync(id, redeem.Uuid, req));
    }
  }

  internal static async Task Dispatch(object sender, ChannelPointsCustomRewardRedemptionArgs args)
  {
    // Let's make sure it's a reward we're actually in charge of first
    var evt = args.Notification.Payload.Event;
    var reward = Redemptions.GetValueOrDefault(evt.Reward.Id);
    if (reward == null) return;

    RewardContext ctx = new RewardContext(args);

    bool allowed = true;
    string failWarning = null;

    foreach (var restriction in reward.Restrictions)
    {
      bool? result = await restriction.PassesCondition(ctx, await JoltCache.GetOwnChannelInfo());
      if (result == true)
      {
        allowed = true;
        failWarning = null;
        if (restriction.StopOnAllow) break;
      }
      else if (result == false)
      {
        allowed = false;
        failWarning = restriction.FailWarning;
        if (restriction.StopOnDeny) break;
      }
    }

    if (!allowed)
    {
      if (failWarning != null) await ctx.ReplyAsync($"You can't use this reward: {failWarning}");
      if (evt.Status == "unfulfilled")
        await JoltApiClient.WithToken((api, id) => api.Helix.ChannelPoints
          .UpdateRedemptionStatusAsync(id, reward.Uuid, [evt.Id],
            new UpdateCustomRewardRedemptionStatusRequest()
            {
              Status = CustomRewardRedemptionStatus.CANCELED
            }));
      return;
    }

    try
    {
      await (Task)reward.Method.Invoke(null, [ctx]);
    }
    catch (TargetInvocationException e)
    {
      await ctx.ReplyAsync($"Error: {e.InnerException.GetType().Name}: {e.InnerException.Message}");
      Logger.LogError(e, "Error in reward execution");
    }
  }

  internal static async Task<string> CreateNewReward(string name)
  {
    var reward = await JoltApiClient.WithToken((api, id) =>
      api.Helix.ChannelPoints.CreateCustomRewardsAsync(id, new CreateCustomRewardsRequest
      {
        Cost = 100,
        IsEnabled = false,
        Prompt = "This was created by Jolt and not updated yet!",
        Title = name
      })
    );

    string id = reward.Data.First().Id;
    TwitchJson.AddReward(name, id);
    TwitchJson.Save();
    return id;
  }
}

public class BotRedemption
{
  public string Uuid { get; init; }
  public MethodInfo Method { get; init; }
  public IEnumerable<LimitAttribute> Restrictions { get; init; }
  public IEnumerable<RewardModifierAttribute> Modifiers { get; init; }
}