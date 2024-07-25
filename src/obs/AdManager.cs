// using Nixill.Streaming.JoltBot.Twitch.Api;
// using NodaTime;
// using TwitchLib.Api.Helix.Models.Channels.GetAdSchedule;

// namespace Nixill.Streaming.JoltBot.OBS;

// public static class AdManager
// {
//   static bool IsInfoKnown = false;
//   static Instant _PreRollsExpire = Instant.MinValue;
//   static Instant PreRollsExpire
//   {
//     get
//     {
//       if (!IsInfoKnown) GetInfo().GetAwaiter().GetResult();
//       return _PreRollsExpire;
//     }
//   }

//   public static int NextAdDuration = 180;
//   public static bool CountdownRunning = false;

//   static Task AdCountdownTimer = null;

//   public static async Task GetInfo()
//   {
//     AdSchedule answer = (await JoltApiClient.WithToken((api, id) => api.Helix.Channels.GetAdScheduleAsync(id))).Data[0];

//     IsInfoKnown = true;

//     if (answer.PrerollFreeTime > 0)
//       _PreRollsExpire = SystemClock.Instance.GetCurrentInstant() + Duration.FromSeconds(answer.PrerollFreeTime);
//   }

//   public static int MaximumAdDuration()
//   {
//     Instant now = SystemClock.Instance.GetCurrentInstant();

//     if (now >= PreRollsExpire) return 180;
//     int tensOfMinutes = (PreRollsExpire - now).Minutes / 10;
//     return 180 - tensOfMinutes * 30;
//   }

//   public static async Task RunAdAfterCountdown()
//   {

//     for (int i = 60; i > 0; i--)
//     {
//       if (!CountdownRunning) return;
//     }
//   }
// }