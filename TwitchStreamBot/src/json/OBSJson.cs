using System.Text.Json.Nodes;

namespace Nixill.Streaming.JoltBot.JSON;

public static class OBSJson
{
  static JsonObject _root;
  public static JsonObject Root
  {
    get =>
      _root ?? (_root = (JsonObject)JsonNode.Parse(File.ReadAllText("data/obs.json")));
  }

  public static class Server
  {
    public static string IP = (string)Root["server"]["ip"];
    public static int Port = (int)Root["server"]["port"];
    public static string Password = (string)Root["server"]["password"];
  }

  public static Dictionary<string, string[]> SceneSwitcher = ((JsonObject)Root["sceneSwitcher"])
    .Select(kvp => (
      kvp.Key,
      ((JsonArray)kvp.Value)
        .Select(s => (string)s)
        .ToArray()))
    .ToDictionary();

  public static string ScreenshotFolder = (string)Root["screenshotFolder"];
}