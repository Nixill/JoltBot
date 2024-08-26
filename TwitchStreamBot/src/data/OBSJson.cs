using System.Text.Json.Nodes;

namespace Nixill.Streaming.JoltBot.Data;

public static class OBSJson
{
  static JsonObject _root;
  public static JsonObject Root
  {
    get => _root ??= (JsonObject)JsonNode.Parse(File.ReadAllText("data/obs.json"));
  }

  public static class Server
  {
    public static readonly string IP = (string)Root["server"]["ip"];
    public static readonly int Port = (int)Root["server"]["port"];
    public static readonly string Password = (string)Root["server"]["password"];
  }

  public static readonly Dictionary<string, int> BottomTextLengths = ((JsonObject)Root["bottomText"])
    .Select(kvp => (
      kvp.Key,
      (int)kvp.Value))
    .ToDictionary();

  public static readonly Dictionary<string, string[]> SceneSwitcher = ((JsonObject)Root["sceneSwitcher"])
    .Select(kvp => (
      kvp.Key,
      ((JsonArray)kvp.Value)
        .Select(s => (string)s)
        .ToArray()))
    .ToDictionary();

  public static readonly string ScreenshotFolder = (string)Root["screenshotFolder"];
}