using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.Data.UFO50;
using Nixill.Streaming.JoltBot.OBS;
using Nixill.Utils.Extensions;
using NodaTime;

namespace Nixill.Streaming.JoltBot.Games.UFO50;

public static class BingoMusicController
{
  static readonly ILogger Logger = JoltBot.Log.Factory.CreateLogger(typeof(BingoMusicController));

  // To be downgraded to "Trace" later.
  delegate void LogFunc(string template, params object[] parameters);
  static readonly LogFunc Log = Logger.LogInformation;

  static List<string> UnplayedTracks = [];

  static BingoMusicState State = BingoMusicState.None;

  // static Instant Now => SystemClock.Instance.GetCurrentInstant();
  // static Instant LastTrackChange = Now;

  public static async Task MusicPlaybackEnded(object sender, InputEventArgs args)
  {
    Log("Media playback ended for {0}", args.InputName);
    switch (args.InputName)
    {
      case "med_UFO50PlayForever": await BeginMenuMusic(); break;
      case "med_UFO50Music": await SelectNextTrack(); break;
    }
  }

  public static async Task PlayIntroMusic()
  {
    Log("Playing Recovery Team music!");

    State = BingoMusicState.Recovery;

    await new OBSRequestBatch([
      OBSExtraRequests.Inputs.Media.SetMediaFile("med_UFO50Music", @$""),
      OBSExtraRequests.Inputs.Media.SetMediaFile("med_UFO50MenuTheme", @$""),
      OBSRequests.MediaInputs.TriggerMediaInputAction("med_UFO50PlayForever", OBSMediaInputAction.Stop),
      OBSRequests.MediaInputs.TriggerMediaInputAction("med_UFO50RecoveryTeam", OBSMediaInputAction.Restart),
      OBSRequests.MediaInputs.TriggerMediaInputAction("med_UFO50RecoveryTeam", OBSMediaInputAction.Play),
      OBSExtraRequests.Inputs.Text.SetText("txt_BingoNowPlaying", "UFO 50 Recovery Team")
    ]).Send();
  }

  public static async Task PlayForever()
  {
    Log("Play Forever!!");

    State = BingoMusicState.Menu;

    UnplayedTracks = MusicJson.GetAllTracks();

    await new OBSRequestBatch([
      .. Enumerable.Select(["med_UFO50Music", "med_UFO50MenuTheme", "med_UFO50RecoveryTeam"],
        s => OBSRequests.MediaInputs.TriggerMediaInputAction(s, OBSMediaInputAction.Stop)),
      OBSRequests.MediaInputs.TriggerMediaInputAction("med_UFO50PlayForever", OBSMediaInputAction.Restart),
      OBSRequests.MediaInputs.TriggerMediaInputAction("med_UFO50PlayForever", OBSMediaInputAction.Play),
      OBSExtraRequests.Inputs.Text.SetText("txt_BingoNowPlaying", "MENU THEME: Play Forever!")
    ]).Send();
  }

  public static async Task BeginMenuMusic()
  {
    Log("Selecting a menu music to play.");

    if (State != BingoMusicState.Menu) return;

    // Select a menu theme at random!
    string menuTheme = MusicJson.SelectMenuTheme();
    string musicTitle = MusicJson.GetTitleOf(menuTheme);

    Log("Playing menu music: {0}", musicTitle);

    await new OBSRequestBatch([
      OBSExtraRequests.Inputs.Media.SetMediaFile("med_UFO50MenuTheme",
        @$"C:\Users\Nixill\Documents\Streaming-2024\Music\ufo50\loops\{menuTheme}.mp3"),
      OBSRequests.General.Sleep(millis: 50),
      OBSExtraRequests.Inputs.Text.SetText("txt_BingoNowPlaying", musicTitle)
    ]).Send();
  }

  public static async Task EndMenuMusic()
  {
    if (State != BingoMusicState.Menu) return;

    Log("Ending menu music as gameplay begins.");

    State = BingoMusicState.Gameplay;

    VolumeLevel volumeLevel = (await OBSRequests.Inputs.GetInputVolume("med_UFO50MenuTheme").Send()).Level;

    await new OBSRequestBatch([
      OBSRequests.Filters.SetSourceFilterEnabled("med_UFO50MenuTheme", "mv_FadeOut", true),
      OBSRequests.General.Sleep(millis: 5_000),
      OBSExtraRequests.Inputs.Media.SetMediaFile("med_UFO50MenuTheme", @$""),
      OBSRequests.General.Sleep(millis: 100),
      OBSRequests.Inputs.SetInputVolume("med_UFO50MenuTheme", volumeLevel),
    ]).Send();

    await SelectNextTrack();
  }

  public static async Task SelectNextTrack()
  {
    await Task.Delay(50);

    var ReturnedState = await OBSRequests.MediaInputs.GetMediaInputStatus("med_UFO50Music").Send();
    var MusicState = ReturnedState.State;
    if (!MusicState.IsStopped() && ReturnedState.Duration != null && ReturnedState.Duration > 100) return;

    Log("Selecting a gameplay song to play.");

    string game = BingoGameChanger.LastGame[Random.Shared.Next() % 2 + 1];

    Log("Selected game {0} {1}", game, UFO50Games.Get(int.Parse(game)));

    string[] songs = [.. MusicJson.GetTracksFor(game)];
    string randomSong = songs[Random.Shared.Next(songs.Length)];
    string trackTitle = MusicJson.GetTitleOf(randomSong);

    Log("Selected song {0}.", trackTitle);

    if (!UnplayedTracks.Contains(randomSong))
    {
      randomSong = UnplayedTracks[Random.Shared.Next(UnplayedTracks.Count)];
      trackTitle = MusicJson.GetTitleOf(randomSong);
      Log("That song was already played. Replacing with song {0}.", trackTitle);
    }

    UnplayedTracks.Remove(randomSong);

    await new OBSRequestBatch([
      OBSExtraRequests.Inputs.Text.SetText("txt_BingoNowPlaying", trackTitle),
      OBSExtraRequests.Inputs.Media.SetMediaFile("med_UFO50Music",
        @$"C:\Users\Nixill\Documents\Streaming-2024\Music\ufo50\ogg\{randomSong}.ogg")
    ]).Send();

    // LastTrackChange = Now;
  }
}

internal enum BingoMusicState
{
  None = 0,
  Recovery = 1,
  Menu = 2,
  Gameplay = 3
}