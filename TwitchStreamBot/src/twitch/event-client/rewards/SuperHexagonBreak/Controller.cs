#define SHB_DEBUG
using Nixill.Streaming.JoltBot.Data;
using Nixill.Streaming.JoltBot.Twitch.Api;
using Nixill.Utils;
using NodaTime;
using TwitchLib.Api.Core.Enums;
using TwitchLib.Api.Helix.Models.ChannelPoints.UpdateCustomReward;
using TwitchLib.Api.Helix.Models.ChannelPoints.UpdateCustomRewardRedemptionStatus;

namespace Nixill.Streaming.JoltBot.Twitch.Events.Rewards;

public static class SuperHexagonController
{
  static Instant Now => SystemClock.Instance.GetCurrentInstant();
  static CancellationTokenSource Timer = null;

  public static async Task SuperHexagonBreak(RewardContext ctx, SuperHexagonLevel level)
  {
    await ctx.MessageAsync("⚠️ Super Hexagon contains spinning and "
      + "flashing lights that may be problematic for some viewers. ⚠️");
    await ctx.MessageAsync("It usually lasts about 3 minutes. Nix's PB "
      + "is just under 5½ minutes.");

    Instant now = Now;

    SuperHexagonJson.Status = SuperHexagonStatus.Waiting;
    SuperHexagonJson.Level = level;
    SuperHexagonJson.LastActive = now;
    SuperHexagonJson.RedeemNum += 1;
    SuperHexagonJson.RedeemPosted = false;
    SuperHexagonJson.RedeemScore = SuperHexagonScore.Zero;
    SuperHexagonJson.LastRedeemID = ctx.RedemptionArgs.Id;
    SuperHexagonJson.LastRedeemerID = ctx.RedemptionArgs.UserId;
    SuperHexagonJson.LastRedeemerUsername = ctx.RedemptionArgs.UserLogin;
    SuperHexagonJson.Played = SuperHexagonJson.Played.Append(level).Distinct().ToArray();
    SuperHexagonJson.Save();

    Task _ = JoltRewardDispatch.Modify();
    _ = SetSHBsPaused(true);

    Timer?.Cancel();
    Timer = new();
    _ = CancelRedemptionAfterOneHour(ctx.RedemptionArgs.Id, level, now, Timer.Token);
  }

  public static async Task Acknowledge(BaseContext ctx)
  {
    if (SuperHexagonJson.Status == SuperHexagonStatus.Waiting)
    {
      Timer?.Cancel();

      await ctx.ReplyAsync("Thank you for acknowledging the redeemed break. I'll reset the timer for you.");

      Timer = new();
      Task _ = CancelRedemptionAfterOneHour(SuperHexagonJson.LastRedeemID, SuperHexagonJson.Level, Now, Timer.Token);
    }
    else if (SuperHexagonJson.Status == SuperHexagonStatus.Active)
    {
      await ctx.ReplyAsync("Can't acknowledge a Super Hexagon Break that's already been scored!");
    }
    else
    {
      await ctx.ReplyAsync("There isn't a waiting Super Hexagon Break.");
    }
  }

  public static async Task Score(BaseContext ctx, SuperHexagonScore score)
  {
    if (SuperHexagonJson.Status == SuperHexagonStatus.Waiting || SuperHexagonJson.Status == SuperHexagonStatus.Active)
    {
      Timer?.Cancel();

      SuperHexagonJson.Status = SuperHexagonStatus.Active;
      SuperHexagonJson.LastActive = Now;
      if (!SuperHexagonJson.RedeemPosted)
      {
        await SuperHexagonCSVs.AddRedemption(new SuperHexagonRedemption
        {
          Date = SuperHexagonJson.StreamDate,
          Level = SuperHexagonJson.Level,
          RedeemerID = SuperHexagonJson.LastRedeemerID,
          RedeemerUsername = SuperHexagonJson.LastRedeemerUsername,
          RedemptionId = SuperHexagonJson.RedeemNum
        });
        SuperHexagonJson.RedeemPosted = true;
      }

      SuperHexagonScore previous = SuperHexagonJson.RedeemScore;
      SuperHexagonScore sum = score + previous;

      SuperHexagonJson.RedeemScore = sum;

      int attempt = SuperHexagonCSVs.Attempts.Last().AttemptId + 1;

      await SuperHexagonCSVs.AddAttempt(new SuperHexagonAttempt
      {
        AttemptId = attempt,
        RedemptionId = SuperHexagonJson.RedeemNum,
        Score = score
      });

      await ctx.ReplyAsync($"Recorded a score of {score} in attempt #{attempt}.");
      bool fulfill = false;

      if (score > SuperHexagonScore.Win)
      {
        await ctx.MessageAsync($"Cleared! Another {SuperHexagonJson.Level} win in the books for the Nix.");
        fulfill = true;
      }
      else if (sum > SuperHexagonScore.Lose)
      {
        await ctx.MessageAsync($"Oh no! That's a total of two minutes ({sum})! It was a valiant effort, but "
          + $"{SuperHexagonJson.Level} has beaten Nix this time.");
        fulfill = true;
      }
      else if (sum > score)
      {
        await ctx.MessageAsync($"That's a total of {sum}, with {SuperHexagonScore.Lose - sum} left in this redemption.");
      }
      else
      {
        await ctx.MessageAsync($"{SuperHexagonScore.Lose - sum} remains in this redemption.");
      }

      if (fulfill)
      {
        Timer = new();
        await JoltApiClient.WithToken((api, id) => api.Helix.ChannelPoints.UpdateRedemptionStatusAsync(id,
          RewardsJson.RewardKeys[$"SuperHexagon.{SuperHexagonJson.Level}"], [SuperHexagonJson.LastRedeemID],
          new UpdateCustomRewardRedemptionStatusRequest { Status = CustomRewardRedemptionStatus.FULFILLED }));

        SuperHexagonJson.LastRedeemID = null;
        SuperHexagonJson.LastRedeemerID = null;
        SuperHexagonJson.LastRedeemerUsername = null;
        SuperHexagonJson.Status = SuperHexagonStatus.Cooldown;

        Task _ = UnpauseRedemptionsAfterHalfHour(Now, Timer.Token);
      }

      SuperHexagonJson.Save();
    }
  }

  static async Task CancelRedemptionAfterOneHour(string redemptionId, SuperHexagonLevel level, Instant startTime,
    CancellationToken token)
  {
    Instant now = Now;

#if SHB_DEBUG
    Instant halfHour = now;
    Instant tenMinutes = now;
#else
    Instant halfHour = startTime + Duration.FromMinutes(30);
    Instant tenMinutes = halfHour + Duration.FromMinutes(20);
#endif
    Instant twoMinutes = tenMinutes + Duration.FromMinutes(8);
    Instant endOfTimer = twoMinutes + Duration.FromMinutes(2);

    try
    {
      if (now < halfHour)
      {
        await Task.Delay(Duration.Min(halfHour - now, Duration.FromMinutes(30)).ToTimeSpan(), token);
        await JoltChatBot.Chat("Hey, Nix! Did you forget about the Super Hexagon Break?");
      }

      if (now < tenMinutes)
      {
        await Task.Delay(Duration.Min(tenMinutes - now, Duration.FromMinutes(10)).ToTimeSpan(), token);
        await JoltChatBot.Chat("Nix, you've still got a Super Hexagon Break to do...");
      }

      if (now < twoMinutes)
      {
        await Task.Delay(Duration.Min(twoMinutes - now, Duration.FromMinutes(8)).ToTimeSpan(), token);
        await JoltChatBot.Chat("Nix, two minute warning on that Super Hexagon Break.");
      }

      if (now < endOfTimer)
      {
        await Task.Delay(Duration.Min(endOfTimer - now, Duration.FromMinutes(2)).ToTimeSpan(), token);
        await JoltChatBot.Chat("That's time! I'll just refund you that Super Hexagon Break redemption.");
      }

      await JoltApiClient.WithToken((api, id) => api.Helix.ChannelPoints.UpdateRedemptionStatusAsync(id,
        RewardsJson.RewardKeys[$"SuperHexagon.{level}"], [redemptionId], new UpdateCustomRewardRedemptionStatusRequest
        {
          Status = TwitchLib.Api.Core.Enums.CustomRewardRedemptionStatus.CANCELED
        }
      ));

      SuperHexagonJson.Played = SuperHexagonJson.Played.Except([level]).ToArray();
      SuperHexagonJson.Status = SuperHexagonStatus.None;
      SuperHexagonJson.LastActive = Now;
      SuperHexagonJson.LastRedeemID = null;
      SuperHexagonJson.RedeemNum -= 1;
      SuperHexagonJson.Save();
    }
    catch (TaskCanceledException) { }
  }

  static async Task UnpauseRedemptionsAfterHalfHour(Instant startTime, CancellationToken token)
  {
    try
    {
      Instant now = Now;
      Instant halfHour = startTime + Duration.FromMinutes(30);

      if (now < halfHour) await Task.Delay((halfHour - now).ToTimeSpan(), token);

      Task _ = SetSHBsPaused(false);
    }
    catch (TaskCanceledException) { }
  }

  static async Task SetSHBsPaused(bool newPauseState)
    => await Enum.GetValues<SuperHexagonLevel>()
      .Select(v => RewardsJson.RewardKeys[$"SuperHexagon.{v}"])
      .Select(uuid => JoltApiClient.WithToken((api, id) => api.Helix.ChannelPoints.UpdateCustomRewardAsync(id, uuid,
        new UpdateCustomRewardRequest { IsPaused = newPauseState }))).WaitAllNoReturn();
}
