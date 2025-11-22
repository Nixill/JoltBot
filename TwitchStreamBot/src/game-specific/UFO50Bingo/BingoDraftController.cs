
using Nixill.OBSWS;
using Nixill.Streaming.JoltBot.OBS;
using Nixill.Utils.Extensions;

namespace Nixill.Streaming.JoltBot.Games.UFO50;

public static class BingoDraftController
{
  internal static bool[][] GamePlayerAssignments = [[], [.. Enumerable.Repeat(false, 51)], [.. Enumerable.Repeat(false, 51)]];
  internal static int[] ProtectedGames = [0, 0, 0];

  /// <remarks>
  ///   The game listed for each player is the game <i>that player chose
  ///   to ban</i>, not the game they originally chose that got banned by
  ///   the other player.
  /// </remarks>
  internal static int[] BannedGames = [0, 0, 0];

  static string GameCartPath(string cart) => $@"C:\Users\Nixill\Documents\Streaming-2024\Images\UFO50\Games\{cart}.png";

  public static async Task DraftGamePressed(int game, int player, UFO50DraftMode mode)
  {
    switch (mode)
    {
      case UFO50DraftMode.Pick: await ToggleDraftGame(game, player); break;
      case UFO50DraftMode.Protect: await ProtectDraftGame(game); break;
      case UFO50DraftMode.Ban: await BanDraftGame(game); break;
    }
  }

  public static async Task SetGameCartImage(int game, string which)
  {
    if (game == 0) return;
    await OBSExtraRequests.Inputs.Image.SetInputImage($"img_UFO50GameCard{game}", GameCartPath(which + game)).Send();
  }

  public static async Task SetGameCartImage(int game)
  {
    if (game == 0) return;
    bool isProtected = ProtectedGames[1] == game || ProtectedGames[2] == game;
    bool isBanned = BannedGames[1] == game || BannedGames[2] == game;
    bool isAssignedP1 = GamePlayerAssignments[1][game];
    bool isAssignedP2 = GamePlayerAssignments[2][game];

    string cart =
      (isProtected ? "g" : isBanned ? "d" :
      isAssignedP1 ? (isAssignedP2 ? "g" : "") : (isAssignedP2 ? "c" : "d")) + game;
    cart = $"{GameCartPath:cart}";

    await OBSExtraRequests.Inputs.Image.SetInputImage($"img_UFO50GameCard{game}", cart).Send();
  }

  public static async Task SetSelectionCartImage(string source, string cart)
    => await OBSExtraRequests.Inputs.Image.SetInputImage($"img_BingoDraftedGame{source}", GameCartPath(cart)).Send();

  public static async Task ToggleDraftGame(int game, int player)
  {
    bool isAssigned = !GamePlayerAssignments[player][game];
    GamePlayerAssignments[player][game] = isAssigned;
    if (ProtectedGames[1] == game) ProtectedGames[1] = 0;
    if (ProtectedGames[2] == game) ProtectedGames[2] = 0;
    if (BannedGames[1] == game) BannedGames[1] = 0;
    if (BannedGames[2] == game) BannedGames[2] = 0;
    await SetGameCartImage(game);
  }

  public static async Task ProtectDraftGame(int game)
  {
    if (GamePlayerAssignments[1][game] == GamePlayerAssignments[2][game]) return;
    int player = GamePlayerAssignments[1][game] ? 1 : 2;
    int unprotected = ProtectedGames[player];
    ProtectedGames[player] = game;
    await SetGameCartImage(game);
    if (unprotected != 0) await SetGameCartImage(unprotected);
  }

  public static async Task BanDraftGame(int game)
  {
    if (GamePlayerAssignments[1][game] == GamePlayerAssignments[2][game]) return;
    int player = GamePlayerAssignments[1][game] ? 2 : 1;
    if (ProtectedGames[3 - player] == game) return;
    int unbanned = BannedGames[player];
    BannedGames[player] = game;
    await SetGameCartImage(game);
    if (unbanned != 0) await SetGameCartImage(unbanned);
  }

  static readonly Dictionary<int, string[]> SharedGameConfigs = new Dictionary<int, string[]>
  {
    [0] = [],
    [1] = ["6"],
    [2] = ["4", "5"],
    [3] = ["1", "2", "3"],
    [4] = ["1", "2", "3", "4"],
    [5] = ["1", "2", "3", "4", "5"],
    [6] = ["1", "2", "3", "4", "5", "6"],
    [7] = ["1", "2", "3", "4", "5", "9", "10"],
    [8] = ["1", "2", "3", "4", "5", "6", "9", "10"],
    [9] = ["1", "2", "3", "4", "5", "7", "8", "9", "10"],
    [10] = ["1", "2", "3", "4", "5", "6", "7", "8", "9", "10"]
  };

  public static async Task FinalizeGames()
  {
    int[] gamesChosenP1 = [.. GamePlayerAssignments[1].Index().Where(i => i.Item && i.Index != BannedGames[2]).Select(i => i.Index)];
    int[] gamesChosenP2 = [.. GamePlayerAssignments[2].Index().Where(i => i.Item && i.Index != BannedGames[1]).Select(i => i.Index)];
    int[] gamesChosenP1Ex = [.. gamesChosenP1.Except(gamesChosenP2)];
    int[] gamesChosenP2Ex = [.. gamesChosenP2.Except(gamesChosenP1)];
    int[] gamesShared = [.. gamesChosenP1.Intersect(gamesChosenP2)];

    // Set game carts on the draft screen to the player colors
    _ = SetGameCartImage(ProtectedGames[1], "");
    _ = SetGameCartImage(ProtectedGames[2], "c");

    foreach (int[] chunk in gamesShared.OrderBy(i => Random.Shared.Next()).WholeChunk(2))
    {
      _ = SetGameCartImage(chunk[0], "");
      _ = SetGameCartImage(chunk[1], "c");
    }

    // Set game selections on the main bingo scene, if one of the
    // following is true:
    // • Per-player games ≤ 7 and shared games ≤ 6
    // • Per-player games plus shared games ≤ 10
    // • Per-player games ≤ 5 and shared games ≤ 10
    int player1Picks = gamesChosenP1Ex.Length;
    int player2Picks = gamesChosenP2Ex.Length;
    int perPlayerPicks = int.Max(player1Picks, player2Picks);
    int sharedPicks = gamesShared.Length;

    if (perPlayerPicks <= 7 && sharedPicks <= 6)
    {
      // Just use slots pp 1-7 and s 1-6!
      gamesChosenP1Ex.Index(1).Select(t => SetSelectionCartImage($"P1G{t.Index}", $"{t.Item}")).NoWait();
      gamesChosenP2Ex.Index(1).Select(t => SetSelectionCartImage($"P2G{t.Index}", $"c{t.Item}")).NoWait();
      gamesShared.Index(1).Select(t => SetSelectionCartImage($"SharedG{t.Index}", $"g{t.Item}")).NoWait();
    }
    else if (perPlayerPicks + sharedPicks <= 10)
    {
      // Uses the shared-slot configs above
      gamesChosenP1Ex.Index(1).Select(t => SetSelectionCartImage($"P1G{t.Index}", $"{t.Item}")).NoWait();
      gamesChosenP2Ex.Index(1).Select(t => SetSelectionCartImage($"P2G{t.Index}", $"c{t.Item}")).NoWait();
      gamesShared.Zip(SharedGameConfigs[sharedPicks]).Select(t => SetSelectionCartImage($"SharedG{t.Second}", $"g{t.First}")).NoWait();
    }
    else if (perPlayerPicks <= 5 && sharedPicks <= 10)
    {
      // Uses player slots 1/2/6/7/4 and shared slots in order
      gamesChosenP1Ex.Zip(["1", "2", "6", "7", "4"]).Select(t => SetSelectionCartImage($"P1G{t.Second}", $"{t.First}")).NoWait();
      gamesChosenP2Ex.Zip(["1", "2", "6", "7", "4"]).Select(t => SetSelectionCartImage($"P2G{t.Second}", $"{t.First}")).NoWait();
      gamesShared.Index(1).Select(t => SetSelectionCartImage($"SharedG{t.Index}", $"g{t.Item}")).NoWait();
    }
    // Otherwise, don't show the game carts on the bingo scene at all.

    // Lastly, hide bingo general goals and show selected carts instead.
    _ = await OBSUtils.SceneItemDisabler("sc_UFO 50 Bingo", "grp_BingoP1GoalScores");
    _ = await OBSUtils.SceneItemDisabler("sc_UFO 50 Bingo", "grp_BingoP2GoalScores");
    _ = await OBSUtils.SceneItemDisabler("sc_UFO 50 Bingo", "grp_BingoGoals");
    _ = await OBSUtils.SceneItemDisabler("sc_UFO 50 Bingo", "grp_BingoGoalsUnrevealed");
    _ = await OBSUtils.SceneItemEnabler("sc_UFO 50 Bingo", "grp_BingoDraftedGames");

    // NOTE TO FUTURE SELF: It is INTENTIONAL that I'm not changing the
    // scene here! I need to see the game selection screen a little longer
    // for setting up the actual draft.
  }

  internal static async Task Reset()
  {
    GamePlayerAssignments = [[], [.. Enumerable.Repeat(false, 51)], [.. Enumerable.Repeat(false, 51)]];
    ProtectedGames = [0, 0, 0];
    BannedGames = [0, 0, 0];
  }
}