// #define SHB_DEBUG
using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.Data;
using Nixill.Streaming.JoltBot.OBS;
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

  private static void StreamStartedHandler(object sender, OutputStateChanged e)
  {
    if (SuperHexagonJson.Status == SuperHexagonStatus.None)
    {
      if (Timer != null)
      {
        Timer.Cancel();
        Timer = null;
      }

      if (SuperHexagonJson.LastActive <= Now - Duration.FromMinutes(90) && MemoryJson.Clock.LastEndTime <= Now - Duration.FromMinutes(30))
      {
        SuperHexagonJson.LastActive = Now;
        SuperHexagonJson.Played = [];
        SuperHexagonJson.StreamDate = Now.InZone(MemoryJson.TimeZone).LocalDateTime.Date;
      }
    }
  }

  private static void StreamStoppedHandler(object sender, OutputStateChanged e)
  {
    if (SuperHexagonJson.Status == SuperHexagonStatus.None)
    {
      Timer = new();
      Task _ = ResetLevelsAfterHalfHour(Now, Timer.Token);
    }
  }

  public static async Task Startup()
  {
    await Task.Delay(TimeSpan.FromSeconds(20));

    Timer?.Cancel(); // just in case

    if (SuperHexagonJson.Status == SuperHexagonStatus.Waiting)
      await CancelRedemptionAfterOneHour(SuperHexagonJson.LastRedeemID, SuperHexagonJson.Level, SuperHexagonJson.LastActive, (Timer = new()).Token);
    else if (SuperHexagonJson.Status == SuperHexagonStatus.Cooldown)
      await UnpauseRewardsAfterHalfHour(SuperHexagonJson.LastActive, (Timer = new()).Token);

    JoltOBSClient.Client.Events.Outputs.StreamStarted += StreamStartedHandler;
    JoltOBSClient.Client.Events.Outputs.StreamStopped += StreamStoppedHandler;
  }

  public static string GetFriendlyTime(SuperHexagonScore score, bool precise = false)
  {
    var frames = score.Frames;
    if (frames < 7200 /* two minutes */) return $"{frames / 60} second{(frames >= 60 && frames < 120 ? "" : "s")}";

    var twelfths = (frames + 15 * 30) / (5 * 60);
    var quarters = (twelfths) / 3;

    var minutes = twelfths / 12;

    var subQuarters = quarters % 4;
    var subSubThirds = twelfths % 3;

    var partialTwelfths = twelfths % (60 * 12); // for plural rules
    var partialMinutes = minutes % 60;
    var hours = minutes / 60;
    var partialHours = hours % 24;
    var days = hours / 24;

    List<string> ret = [];
    if (days == 1) ret.Add("1 day");
    else if (days > 1) ret.Add($"{days} days");

    if (partialHours == 1) ret.Add("1 hour");
    else if (partialHours > 1) ret.Add($"{partialHours} hours");

    var minutesAndQuarters = $"{(partialMinutes != 0 ? partialMinutes : "")}{subQuarters switch
    {
      1 => "¼",
      2 => "½",
      3 => "¾",
      _ => ""
    }} minute{(partialTwelfths > 12 ? "s" : "")}";
    if (minutesAndQuarters != " minute") ret.Add(minutesAndQuarters);

    if (precise) return $"{subSubThirds switch
    {
      0 => "just under ",
      2 => "just over ",
      _ => ""
    }}{ret.SJoin(", ")}";
    else return ret.SJoin(", ");
  }

  public static async Task SuperHexagonBreak(RewardContext ctx, SuperHexagonLevel level)
  {
    await ctx.MessageAsync("⚠️ Super Hexagon contains spinning and flashing lights that may be problematic for some "
      + "viewers. ⚠️");

    var redemptionsOfLevel = SuperHexagonCSVs.Redemptions.Where(r => r.Level == level);
    var averageTime = GetFriendlyTime(redemptionsOfLevel
      .Select(r => r.GetAttempts().Select(r => r.Score).Sum())
      .Average(SuperHexagonScore.Zero)
    );
    var bestTime = redemptionsOfLevel
      .SelectMany(r => r.GetAttempts())
      .Max(a => a.Score);
    await ctx.MessageAsync($"It usually lasts about {averageTime}. Nix's PB is {bestTime} ({GetFriendlyTime(bestTime,
      true)}).");

    var redemptionsOfUser = SuperHexagonCSVs.Redemptions.Where(r => r.RedeemerID == ctx.UserId);
    var totalTimeSpent = redemptionsOfUser
      .SelectMany(r => r.GetAttempts().Select(a => a.Score))
      .Sum();
    await ctx.MessageAsync($"{ctx.UserName} has redeemed {totalTimeSpent} ({GetFriendlyTime(totalTimeSpent)}) of Super "
      + "Hexagon Breaks.");

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
          RedemptionID = SuperHexagonJson.RedeemNum
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
        RedemptionID = SuperHexagonJson.RedeemNum,
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

        Task _ = UnpauseRewardsAfterHalfHour(Now, Timer.Token);
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

  static async Task UnpauseRewardsAfterHalfHour(Instant startTime, CancellationToken token)
  {
    try
    {
      Instant now = Now;
      Instant minutes25 = startTime + Duration.FromMinutes(25);

      if (now < minutes25) await Task.Delay((minutes25 - now).ToTimeSpan(), token);

      Task _ = SetSHBsPaused(false);
    }
    catch (TaskCanceledException) { }
  }

  static async Task ResetLevelsAfterHalfHour(Instant startTime, CancellationToken token)
  {
    try
    {
      Instant now = Now;
      Instant halfHour = startTime + Duration.FromMinutes(30);

      if (now < halfHour) await Task.Delay((halfHour - now).ToTimeSpan(), token);

      SuperHexagonJson.Played = [];
      SuperHexagonJson.Save();
      await JoltRewardDispatch.Modify();
    }
    catch (TaskCanceledException) { }
  }

  static async Task SetSHBsPaused(bool newPauseState)
    => await Enum.GetValues<SuperHexagonLevel>()
      .Where(v => RewardsJson.RewardKeys.ContainsKey($"SuperHexagon.{v}"))
      .Select(v => RewardsJson.RewardKeys[$"SuperHexagon.{v}"])
      .Select(uuid => JoltApiClient.WithToken((api, id) => api.Helix.ChannelPoints.UpdateCustomRewardAsync(id, uuid,
        new UpdateCustomRewardRequest { IsPaused = newPauseState }))).WaitAllNoReturn();
}
