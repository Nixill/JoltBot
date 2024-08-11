using Microsoft.Extensions.Logging;
using Nixill.Collections;
using Nixill.Streaming.JoltBot.JSON;
using Nixill.Utils;
using NodaTime;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;
using TwitchLib.EventSub.Core.SubscriptionTypes.Channel;

namespace Nixill.Streaming.JoltBot.Twitch.Api;

public static class JoltCache
{
  static ILogger Logger = Log.Factory.CreateLogger(typeof(JoltCache));

  static Instant Now => SystemClock.Instance.GetCurrentInstant();
  static readonly Duration OneHour = Duration.FromHours(1);
  static readonly Duration OneDay = Duration.FromDays(1);

  static async Task<TResult> AndThen<TTask, TResult>(this Task<TTask> task, Func<TTask, TResult> andThen)
    => andThen(await task);

  #region Own Channel Info
  static Task<ChannelInformation> _OwnChannelInfo;
  static Instant _Update_OwnChannelInfo = Instant.MinValue;

  public static async Task<ChannelInformation> GetOwnChannelInfo()
  {
    if (Now > _Update_OwnChannelInfo + OneHour)
    {
      Logger.LogInformation("Channel information expired.");
      UpdateOwnChannelInfo();
    }

    return await _OwnChannelInfo;
  }

  public static void UpdateOwnChannelInfo()
  {
    Logger.LogInformation("Refreshing channel information...");
    _Update_OwnChannelInfo = Now;
    _OwnChannelInfo = JoltApiClient.WithToken(
          (api, id) => api.Helix.Channels.GetChannelInformationAsync(id)
        ).AndThen(rsp => rsp.Data.Where(i => i.BroadcasterId == TwitchJson.Channel.UserId).First());
  }
  #endregion

  #region User Lists
  static Instant _Update_UserLists = Instant.MinValue;

  static Task<Dictionary<string, int>> _Subscribers;
  static Task<HashSet<string>> _Moderators;
  static Task<HashSet<string>> _VIPs;
  static Task<HashSet<string>> _Editors;

  public static void UpdateUserLists()
  {
    _Update_UserLists = Now;

    _Subscribers = Task.Run(async () =>
    {
      Dictionary<string, int> subs = [];
      string cursor = null;

      while (true)
      {
        var result = await JoltApiClient.WithToken((api, id) => api.Helix.Subscriptions
          .GetBroadcasterSubscriptionsAsync(id, first: 100, after: cursor));

        result.Data.Do(s => subs.Add(s.UserId, int.Parse(s.Tier) / 1000));
        if (result.Pagination.Cursor != null) cursor = result.Pagination.Cursor;
        else break;
      }

      return subs;
    });

    _Moderators = Task.Run(async () =>
    {
      HashSet<string> mods = [];
      string cursor = null;

      while (true)
      {
        var result = await JoltApiClient.WithToken((api, id) => api.Helix.Moderation
          .GetModeratorsAsync(id, first: 100, after: cursor));

        result.Data.Do(s => mods.Add(s.UserId));
        if (result.Pagination.Cursor != null) cursor = result.Pagination.Cursor;
        else break;
      }

      return mods;
    });

    _VIPs = Task.Run(async () =>
    {
      HashSet<string> vips = [];
      string cursor = null;

      while (true)
      {
        var result = await JoltApiClient.WithToken((api, id) => api.Helix.Channels
          .GetVIPsAsync(id, first: 100, after: cursor));

        result.Data.Do(s => vips.Add(s.UserId));
        if (result.Pagination.Cursor != null) cursor = result.Pagination.Cursor;
        else break;
      }

      return vips;
    });

    _Editors = JoltApiClient.WithToken((api, id) => api.Helix.Channels.GetChannelEditorsAsync(id))
      .AndThen(result => result.Data.Select(ed => ed.UserId).ToHashSet());
  }

  public static async Task<TwitchUserGroup> GetUserGroup(string uid)
  {
    if (Now > _Update_UserLists + OneHour * 3) UpdateUserLists();

    bool isBroadcaster = uid == TwitchJson.Channel.UserId;

    TwitchUserGroup group =
      (isBroadcaster ? TwitchUserGroup.AllBroadcasterRoles : 0)
      | ((await _Editors).Contains(uid) ? TwitchUserGroup.Editor : 0)
      | ((await _Moderators).Contains(uid) ? TwitchUserGroup.AllModeratorRoles : 0)
      | ((await _VIPs).Contains(uid) ? TwitchUserGroup.VIP : 0)
      | ((await _Subscribers).ContainsKey(uid) ? (await _Subscribers)[uid] switch
      {
        1 => TwitchUserGroup.Tier1Subscriber,
        2 => TwitchUserGroup.Tier2AndBelow,
        3 => TwitchUserGroup.Tier3AndBelow,
        _ => 0
      } : 0)
      | (false ? TwitchUserGroup.Regular : 0)
      | TwitchUserGroup.Anyone;

    return group;
  }
  #endregion
}
