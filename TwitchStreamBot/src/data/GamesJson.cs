using System.Text.Json.Nodes;
using Nixill.Colors;

namespace Nixill.Streaming.JoltBot.Data;

public static class GamesJson
{
  static JsonObject _root;
  public static JsonObject Root
  {
    get => _root ??= (JsonObject)JsonNode.Parse(File.ReadAllText("data/games.json"));
  }

  public static void Save()
  {
    File.WriteAllText("data/games.json", Root.ToString());
  }

  public static JsonObject GameObject(string gameName)
    => (JsonObject)Root.FirstOrDefault(kvp => kvp.Key.Equals(gameName, StringComparison.CurrentCultureIgnoreCase)).Value;

  public static bool IsTitleIgnored(string gameName)
  {
    JsonNode node = GameObject(gameName)?["titleIgnored"];
    return (node != null && node.GetValueKind() == System.Text.Json.JsonValueKind.True);
  }

  public static string[] GetAliases(string gameName)
  {
    JsonNode node = GameObject(gameName)?["aliases"];
    if (node != null && node.GetValueKind() == System.Text.Json.JsonValueKind.Array)
      return ((JsonArray)node).Select(x => (string)x).ToArray();
    return [];
  }

  public static Color GetGameColor(string gameName)
  {
    string str = (string)GameObject(gameName)?["color"];
    if (str != null) return Color.FromRGBA(str);
    return Color.FromRGBA("b42b42");
  }

  public static void SetGameColor(string gameName, Color color)
  {
    JsonObject obj = GameObject(gameName);
    if (obj != null) obj["color"] = color.ToRGBHex();
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
    if (GetAliases(gameName).Any(a => streamTitle.Contains(a))) return true;

    // No? Then it's not a valid title.
    return false;
  }
}