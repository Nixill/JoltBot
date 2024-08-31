using System.Data;
using Nixill.Collections.Grid;
using Nixill.Collections.Grid.CSV;
using Nixill.Colors;

namespace Nixill.Streaming.JoltBot.Data;

public static class GamesCsv
{
  public static readonly Color DefaultColor = Color.FromRGBA("#b42b42");

  static DataTable _table;
  public static DataTable Table
  {
    get => _table ??= DataTableCSVParser.FileToDataTable("data/games.csv",
      [
        new DataColumn("gameName", typeof(string)),
        new DataColumn("color", typeof(Color)) { DefaultValue = Color.FromRGBA("#b42b42") },
        new DataColumn("aliases", typeof(string[])) { DefaultValue = Array.Empty<string>() },
        new DataColumn("ignoreTitle", typeof(bool)) { DefaultValue = false }
      ], new Dictionary<Type, Func<string, object>>().AddArrayDeserializer(";"),
      ["gameName"]
    );
  }

  public static void Save()
  {
    DataTableCSVParser.DataTableToFile(Table, "data/games.csv",
      new Dictionary<Type, Func<object, string>>().AddArraySerializer(";"));
  }

  public static DataRow GameRow(string gameName)
    => Table.AsEnumerable()
      .FirstOrDefault(r => ((string)r["gameName"]).Equals(gameName, StringComparison.InvariantCultureIgnoreCase));

  public static DataRow NewGameRow(string gameName)
  {
    var gameRow = GameRow(gameName);
    if (gameRow == null)
    {
      gameRow = Table.NewRow();
      gameRow["gameName"] = gameName;
      Table.Rows.Add(gameRow);
    }
    return gameRow;
  }

  public static bool IsTitleIgnored(string gameName)
    => (bool?)GameRow(gameName)?["ignoreTitle"] ?? false;

  public static string[] GetAliases(string gameName)
    => (string[])GameRow(gameName)?["aliases"] ?? [];

  public static Color GetGameColor(string gameName)
    => (Color?)GameRow(gameName)?["color"] ?? DefaultColor;

  public static void SetGameColor(string gameName, Color color)
  {
    NewGameRow(gameName)["color"] = color;
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