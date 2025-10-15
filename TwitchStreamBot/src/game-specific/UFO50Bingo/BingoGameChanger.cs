using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.OBS;
using Nixill.Streaming.JoltBot.Pipes;
using Nixill.Utils;
using Quartz.Xml.JobSchedulingData20;

namespace Nixill.Streaming.JoltBot.Games.UFO50;

public static class BingoGameChanger
{
  public static int LastPlayerChanged { get; set; } = 1;

  internal static string[] LastGame = ["" /* unused */, "0", "0"];
  static bool[] GameOnScreen = [false /* unused */, false, false];

  static string[][] GameFrames = [
    [ /* this array intentionally left blank */ ],
    [.. Enumerable.Repeat("blank", 51)],
    [.. Enumerable.Repeat("blank", 51)]
  ];

  public static async Task SwitchToGame(int game, bool terminal)
  {
    int player = LastPlayerChanged;

    if (GameOnScreen[player] && LastGame[player] == $"{(terminal ? "t" : "")}{game}") return;
    if (GameOnScreen[player]) await CloseGameFor(player);
    if (game != 0) await OpenGameFor(player, game, terminal);
  }

  static async Task CloseGameFor(int player)
  {
    GameOnScreen[player] = false;

    await new OBSRequestBatch([
      await OBSUtils.SceneItemDisabler("grp_BingoCurrentGamesClose", $"img_BingoP{player}CurrentGame"),
      await OBSUtils.SceneItemDisabler("grp_BingoCurrentGamesClose", $"img_BingoP{player}Frame-0"),
      OBSRequests.Filters.SetSourceFilterEnabled($"grp_BingoP{player}HistoryOuter", "mv_ToTheRight", true)
    ]).Send();

    await Task.Delay(millisecondsDelay: 600);
  }

  static async Task OpenGameFor(int player, int game, bool terminal)
  {
    GameOnScreen[player] = true;
    int closeGroupSceneItemID = await OBSUtils.GetSceneItemIndex("grp_BingoCurrentGamesClose", $"img_BingoP{player}CurrentGame");
    int closeGroupFrameID = await OBSUtils.GetSceneItemIndex("grp_BingoCurrentGamesClose", $"img_BingoP{player}Frame-0");

    _ = MarkerButton.Place($"Player {player} opened {UFO50Games.Get(game)}");
    string frame = terminal ? "blank" : GameFrames[player][game];

    if ($"{(terminal ? "t" : "")}{game}" == LastGame[player])
    {
      int sceneItemID = await OBSUtils.GetSceneItemIndex("grp_BingoCurrentGamesReopen", $"img_BingoP{player}CurrentGame");
      int frameID = await OBSUtils.GetSceneItemIndex("grp_BingoCurrentGamesReopen", $"img_BingoP{player}Frame-0");
      await new OBSRequestBatch([
        OBSRequests.SceneItems.SetSceneItemEnabled("grp_BingoCurrentGamesReopen", sceneItemID, true),
        OBSRequests.SceneItems.SetSceneItemEnabled("grp_BingoCurrentGamesReopen", frameID, true),
        OBSRequests.Filters.SetSourceFilterEnabled($"grp_BingoP{player}HistoryOuter", "mv_ToTheLeft", true),
        OBSRequests.General.Sleep(millis: 600),
        OBSRequests.SceneItems.SetSceneItemEnabled("grp_BingoCurrentGamesReopen", sceneItemID, false),
        OBSRequests.SceneItems.SetSceneItemEnabled("grp_BingoCurrentGamesClose", closeGroupSceneItemID, true),
        OBSRequests.SceneItems.SetSceneItemEnabled("grp_BingoCurrentGamesReopen", frameID, false),
        OBSRequests.SceneItems.SetSceneItemEnabled("grp_BingoCurrentGamesClose", closeGroupFrameID, true)
      ]).Send();
    }
    else
    {
      int sceneItemID = await OBSUtils.GetSceneItemIndex("grp_BingoCurrentGamesOpen", $"img_BingoP{player}CurrentGame");
      int frameID = await OBSUtils.GetSceneItemIndex("grp_BingoCurrentGamesOpen", $"img_BingoP{player}Frame-0");

      Task copy1 = OBSUtils.CopyImages([$"img_BingoP{player}CurrentGame", .. Enumerable.Range(1, 6).Select(i => $"img_BingoP{player}Game-{i}")],
        [.. Enumerable.Range(1, 7).Select(i => $"img_BingoP{player}Game-{i}")]);
      Task copy2 = OBSUtils.CopyImages([.. Enumerable.Range(0, 7).Select(i => $"img_BingoP{player}Frame-{i}")],
        [.. Enumerable.Range(1, 7).Select(i => $"img_BingoP{player}Frame-{i}")]);

      await copy1;
      await copy2;

      await new OBSRequestBatch([
        await OBSUtils.WithSceneItemIndex(OBSRequests.SceneItems.SetSceneItemTransform, $"grp_BingoP{player}HistoryOuter",
          $"cln_grp_BingoP{player}HistoryInner", new SceneItemTransformSetter { PositionX = 0 }),
        OBSRequests.SceneItems.SetSceneItemEnabled("grp_BingoCurrentGamesOpen", sceneItemID, true),
        OBSRequests.SceneItems.SetSceneItemEnabled("grp_BingoCurrentGamesOpen", frameID, true),
        OBSExtraRequests.Inputs.Image.SetInputImage($"img_BingoP{player}CurrentGame",
          @$"C:\Users\Nixill\Documents\Streaming-2024\Images\UFO50\Games\{(terminal? "t" : "")}{game}.png"),
        OBSExtraRequests.Inputs.Image.SetInputImage($"img_BingoP{player}Frame-0",
          @$"C:\Users\Nixill\Documents\Streaming-2024\Images\UFO50\Games\{frame}.png"),
        OBSRequests.General.Sleep(millis: 600),
        OBSRequests.SceneItems.SetSceneItemEnabled("grp_BingoCurrentGamesOpen", sceneItemID, false),
        OBSRequests.SceneItems.SetSceneItemEnabled("grp_BingoCurrentGamesOpen", frameID, false),
        OBSRequests.SceneItems.SetSceneItemEnabled("grp_BingoCurrentGamesClose", closeGroupSceneItemID, true),
        OBSRequests.SceneItems.SetSceneItemEnabled("grp_BingoCurrentGamesClose", closeGroupFrameID, true)
      ]).Send();

      LastGame[player] = $"{(terminal ? "t" : "")}{game}";
    }

    await BingoMusicController.EndMenuMusic();
  }

  public static async Task UpgradeCartridge(int player)
  {
    if (!int.TryParse(LastGame[player], out int forGame)) return;

    string cartridge = GameFrames[player][forGame];

    cartridge = cartridge switch
    {
      "blank" => "gold",
      "gold" => "cherry",
      "cherry" => "blank",
      _ => "blank"
    };

    await new OBSRequestBatch([
      await OBSUtils.SceneItemEnabler("sc_UFO 50 Bingo", $"img_BingoP{player}FrameFlash"),
      OBSRequests.General.Sleep(millis: 50),
      await OBSUtils.SceneItemDisabler("sc_UFO 50 Bingo", $"img_BingoP{player}FrameFlash"),
      OBSExtraRequests.Inputs.Image.SetInputImage($"img_BingoP{player}Frame-0",
        @$"C:\Users\Nixill\Documents\Streaming-2024\Images\UFO50\Games\{cartridge}.png")
    ]).Send();

    GameFrames[player][forGame] = cartridge;
  }

  public static void Reset()
  {
    LastGame = ["" /* unused */, "0", "0"];
    GameOnScreen = [false /* unused */, false, false];

    GameFrames = [
      [ /* this array intentionally left blank */ ],
      [.. Enumerable.Repeat("blank", 51)],
      [.. Enumerable.Repeat("blank", 51)]
    ];
  }
}