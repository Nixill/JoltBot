using System.Text.Json.Nodes;

namespace Nixill.Streaming.JoltBot.JSON;

public static class AliasesJson
{
  static JsonObject _root;
  public static JsonObject Root
  {
    get =>
      _root ?? (_root = (JsonObject)JsonNode.Parse(File.ReadAllText("data/aliases.json")));
  }

  public static void ReadAliases()
  {
    _root = (JsonObject)JsonNode.Parse(File.ReadAllText("data/aliases.json").ToLower());
  }

  public static bool IsTitleIgnored(string gameName)
  {
    JsonNode node = Root[gameName.ToLower()];
    return (node != null && node.GetValueKind() == System.Text.Json.JsonValueKind.False);
  }

  public static string[] GetAliases(string gameName)
  {
    JsonNode node = Root[gameName.ToLower()];
    if (node != null && node.GetValueKind() == System.Text.Json.JsonValueKind.Array)
      return ((JsonArray)node).Select(x => (string)x).ToArray();
    return new string[] { };
  }

  public static bool IsValidTitle(string gameName, string streamTitle)
  {
    ReadAliases();

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