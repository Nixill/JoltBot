using System.Text.Json.Nodes;

namespace Nixill.Streaming.JoltBot.JSON;

public static class GamesJson
{
  static JsonObject _root;
  public static JsonObject Root
  {
    get => _root ??= (JsonObject)JsonNode.Parse(File.ReadAllText("data/games.json"));
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

  public static string GetGameColor(string gameName)
  {
    JsonNode node = GameObject(gameName)?["color"];
    return (string)node ?? "b42b42";
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