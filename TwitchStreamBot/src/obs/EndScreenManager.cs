using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Microsoft.Extensions.Logging;
using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.JSON;
using Nixill.Streaming.JoltBot.Twitch.Api;
using NodaTime;
using NodaTime.Text;
using TwitchLib.Client.Events;
using TwitchLib.EventSub.Websockets.Core.EventArgs;
using TwitchLib.EventSub.Websockets.Core.EventArgs.Channel;

namespace Nixill.Streaming.JoltBot.OBS;

public static class EndScreenManager
{
  static ILogger Logger = Log.Factory.CreateLogger(typeof(EndScreenManager));

  public static readonly string GameIconFolder = UpcomingJson.GameIconFolder;
  static readonly LocalDatePattern FormatPattern = LocalDatePattern.CreateWithInvariantCulture("ddd., MMM. d");

  public static async Task UpdateOnStartup()
  {
    await UpdateStreamData(null);
    while (!JoltOBSClient.IsConnected) await Task.Delay(5000);
    await UpdateStreamScene();
  }

  public static async Task UpdateStreamData(LocalDate? dat)
  {
    var date = dat ?? LocalDate.FromDateTime(DateTime.Today);
    var now = dat?.ToDateTimeUnspecified() ?? DateTime.Now;

    var calendarStream = new StreamReader(await new HttpClient().GetStreamAsync(UpcomingJson.CalendarLink));

    Calendar iCal = Calendar.Load(calendarStream.ReadToEnd());

    var events = iCal.Calendar.Events;

    Dictionary<(IDateTime Time, string UID), (CalendarEvent Event, Occurrence Occurrence)> occurrences = new();
    HashSet<(IDateTime Time, string UID)> recurrences = new();

    var today = date.ToDateTimeUnspecified();

    foreach (var evt in events)
    {
      var set = evt.GetOccurrences(today.AddDays(0), today.AddDays(22));
      foreach (var occ in set)
      {
        var time = occ.Period.StartTime;
        var recID = evt.RecurrenceId;
        var val = (evt, occ);
        if (recID != null)
        {
          recurrences.Add((recID, evt.Uid));
          occurrences.Remove((recID, evt.Uid));
          occurrences.Add((time, evt.Uid), val);
        }
        else
        {
          if (!recurrences.Contains((time, evt.Uid)))
            occurrences.Add((time, evt.Uid), val);
        }
      }
    }

    List<JsonObject> list = new();

    foreach (int i in Enumerable.Range(0, 22))
    {
      DateTime thisDate = today.AddDays(i);

      var streamsToday = occurrences.Where(kvp => kvp.Key.Time.AsSystemLocal >= thisDate
          && kvp.Key.Time.AsSystemLocal > now.AddMinutes(30)
          // plus 30 because I usually open OBS and stuff within 30
          // minutes *before* the start of stream, and this is likely to
          // be the only update
          && kvp.Key.Time.AsSystemLocal < thisDate.AddDays(1))
        .OrderBy(kvp => kvp.Key.Time);

      if (streamsToday.Any())
      {
        var evt = streamsToday.First();
        JsonObject streamInfo = new();
        streamInfo["date"] = evt.Key.Time.Value.ToString("yyyy-MM-dd");
        streamInfo["name"] = evt.Value.Event.Summary;
        streamInfo["game"] = evt.Value.Event.Description.Split("\n")
          .FirstOrDefault(s => s.ToLower().StartsWith("game: "), $"game: {evt.Value.Event.Summary}")[6..].Trim();
        streamInfo["channel"] = evt.Value.Event.Location;
        streamInfo["andMore"] = (streamsToday.Count() > 1);

        list.Add(streamInfo);

        if (list.Count >= 2) break;
      }
    }

    UpcomingJson.Second = null;
    UpcomingJson.First = null;

    if (list.Count >= 1) UpcomingJson.First = new(list[0]);
    if (list.Count >= 2) UpcomingJson.Second = new(list[1]);

    UpcomingJson.Save();
  }

  public static async Task UpdateStreamScene()
  {
    await UpdateHalfStreamScene(1, UpcomingJson.First);
    await UpdateHalfStreamScene(2, UpcomingJson.Second);
  }

  public static async Task UpdateHalfStreamScene(int i, UpcomingStream? stream)
  {
    var inputIDsRequest = new OBSRequestBatch(new string[] { $"grp_UpcomingGame{i}", $"txt_UpcomingGame{i}", $"txt_UpcomingDate{i}", $"txt_ChannelUrl{i}" }
      .Select(name => OBSRequests.SceneItems.GetSceneItemId("sc_Raiding Screen", name)));
    if (stream == null)
    {
      new OBSRequestBatch((await inputIDsRequest.Send())
        .Select(resp => (int)(OBSSingleValueResult<int>)resp.RequestResult)
        .Select(siid => OBSRequests.SceneItems.SetSceneItemEnabled("sc_Raiding Screen", siid, false)))
      .SendWithoutWaiting();
    }
    else
    {
      new OBSRequestBatch((await inputIDsRequest.Send())
        .Select(resp => (int)(OBSSingleValueResult<int>)resp.RequestResult)
        .Select(siid => OBSRequests.SceneItems.SetSceneItemEnabled("sc_Raiding Screen", siid, true)))
      .SendWithoutWaiting();

      int andMoreID = await OBSRequests.SceneItems.GetSceneItemId($"grp_UpcomingGame{i}", "img_AndMore").Send();
      int channelID = await OBSRequests.SceneItems.GetSceneItemId($"sc_Raiding Screen", $"txt_ChannelUrl{i}").Send();

      var gameIcon = GenerateSlug(stream.Value.Game);
      if (!File.Exists($"{GameIconFolder}{gameIcon}.png")) gameIcon = "unknown";

      bool isToday = stream.Value.Date == LocalDate.FromDateTime(DateTime.Today);
      string dateStr;
      if (isToday)
        dateStr = "Today!";
      else
        dateStr = FormatPattern.Format(stream.Value.Date) + (stream.Value.Date.Day switch
        {
          1 or 21 or 31 => "st",
          2 or 22 => "nd",
          3 or 23 => "rd",
          _ => "th"
        });

      new OBSRequestBatch(new OBSRequest[] {
        OBSExtraRequests.Inputs.Text.SetInputText($"txt_UpcomingGame{i}", stream.Value.Name),
        OBSExtraRequests.Inputs.Image.SetInputImage($"img_UpcomingGame{i}", $"{GameIconFolder}{gameIcon}.png"),
        OBSExtraRequests.Inputs.Text.SetInputText($"txt_UpcomingDate{i}", dateStr),
        OBSExtraRequests.Inputs.Text.SetInputText($"txt_ChannelUrl{i}", stream.Value.Channel),
        OBSRequests.SceneItems.SetSceneItemEnabled("sc_Raiding Screen", channelID, stream.Value.Channel.ToLowerInvariant() switch
        {
          "nixillshadowfox" or "https://twitch.tv/nixillshadowfox" or "https://twitch.nixill.net/" => false,
          _ => true
        }),
        OBSRequests.SceneItems.SetSceneItemEnabled($"grp_UpcomingGame{i}", andMoreID, stream.Value.AndMore)
      }).SendWithoutWaiting();
    }
  }

  public static string RemoveAccent(string text)
  {
    var normalizedString = text.Normalize(NormalizationForm.FormD);
    var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

    for (int i = 0; i < normalizedString.Length; i++)
    {
      char c = normalizedString[i];
      var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
      if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
      {
        stringBuilder.Append(c);
      }
    }

    return stringBuilder
        .ToString()
        .Normalize(NormalizationForm.FormC);
  }

  public static string GenerateSlug(string phrase)
  {
    string str = RemoveAccent(phrase).ToLower();
    // invalid chars           
    str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
    // convert multiple spaces into one space   
    str = Regex.Replace(str, @"\s+", " ").Trim();
    // cut and trim 
    str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
    str = Regex.Replace(str, @"\s", "-"); // hyphens   
    return str;
  }

  public static async Task OnCancelRaid(OnUnraidNotificationArgs e)
  {
    await SceneSwitcher.SwitchTo("sc_Raiding Screen", "txt_LoadingRaid1", "txt_LoadingRaid2");
  }

  public static async Task PrepareRaid(string user, string targetPfp)
  {
    await new OBSRequestBatch(
      OBSExtraRequests.Inputs.Text.SetInputText("txt_RaidUsername", user),
      OBSExtraRequests.Inputs.Browser.SetBrowserURL("brs_RaidTargetIcon", targetPfp)
    ).Send();
    await SceneSwitcher.SwitchTo("sc_Raiding Screen", "txt_RaidFound", "cln_grp_RaidTarget");
  }

  public static async Task OnCompleteRaid()
  {
    await OBSRequests.Stream.StopStream().Send();
  }

  internal static async Task OnCreateRaid(TwitchLib.EventSub.Core.SubscriptionTypes.Channel.ChannelModerate ev)
  {
    var pfp = (await JoltApiClient.WithToken((api, id) =>
      api.Helix.Users.GetUsersAsync(ids: new List<string> { ev.Raid.UserId })))
      .Users.First().ProfileImageUrl;

    await PrepareRaid(ev.Raid.UserName, pfp);
  }
}