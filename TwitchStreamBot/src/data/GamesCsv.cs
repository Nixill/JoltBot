using System.Data;
using Nixill.Collections;
using Nixill.Colors;

namespace Nixill.Streaming.JoltBot.Data;

public static class GamesCsv
{
  static readonly CSVObjectDictionary<string, GameInfo> Games = CSVObjectDictionary.ParseObjectsFromFile("data/games.csv", GameInfo.Parse);

  public static void Save()
  {
    Games.FormatCSVToFile("data/games.csv", GameInfo.UnparseKVP);
  }

  public static GameInfo GetGame(string gameName)
    => Games.TryGetValue(gameName.ToLower(), out GameInfo info) ? info : null;

  public static GameInfo GetOrCreateGame(string gameName)
  {
    if (Games.TryGetValue(gameName.ToLower(), out GameInfo info))
      return info;

    info = new GameInfo { GameName = gameName };
    Games.Add(gameName.ToLower(), info);
    return info;
  }

  public static bool IsTitleIgnored(string gameName)
    => GetGame(gameName)?.IsTitleIgnored ?? false;

  public static List<string> GetAliases(string gameName)
    => GetGame(gameName)?.Aliases ?? [];

  public static Color GetGameColor(string gameName)
    => GetGame(gameName)?.GameColor ?? Color.FromRGBA("#b42b42");

  public static void SetGameColor(string gameName, Color color)
  {
    GetOrCreateGame(gameName).GameColor = color;
    Save();
  }

  public static bool IsValidTitle(string gameName, string streamTitle)
  {
    streamTitle = streamTitle.ToLower();
    gameName = gameName.ToLower();

    // 1. Does the stream title contain the game's name literally?
    if (streamTitle.Contains(gameName)) return true;

    // 2. Is this game title ignored?
    if (IsTitleIgnored(gameName)) return true;

    // 3. Does the stream title contain any of the game's aliases?
    if (GetAliases(gameName).Any(a => streamTitle.Contains(a, StringComparison.InvariantCultureIgnoreCase))) return true;

    // No? Then it's not a valid title.
    return false;
  }
}

public class GameInfo
{
  public static readonly Color DefaultColor = Color.FromRGBA("#b42b42");

  public required string GameName { get; init; }
  public List<string> Aliases { get; private init; } = [];
  public IEnumerable<string> InitAliases { init { Aliases = [.. value]; } }
  public bool IsTitleIgnored { get; set; } = false;
  public Color GameColor { get; set; } = DefaultColor;

  public static KeyValuePair<string, GameInfo> Parse(IDictionary<string, string> dictionary)
  {
    return new KeyValuePair<string, GameInfo>(
      dictionary["gameName"].ToLower(),
      new GameInfo
      {
        GameName = dictionary["gameName"],
        InitAliases = dictionary["aliases"]?.Split(';') ?? [],
        IsTitleIgnored = dictionary["ignoreTitle"] == "true",
        GameColor = Color.FromRGBA(dictionary["color"] ?? "b42b42")
      }
    );
  }

  internal IDictionary<string, string> Unparse()
  {
    string test = GameColor.ToRGBHex();
    return new Dictionary<string, string>
    {
      ["gameName"] = GameName,
      ["color"] = test != "#b42b42" ? test : "",
      ["aliases"] = string.Join(';', Aliases),
      ["ignoreTitle"] = IsTitleIgnored ? "true" : ""
    };
  }

  internal static IDictionary<string, string> UnparseKVP(KeyValuePair<string, GameInfo> kvp)
    => kvp.Value.Unparse();
}
