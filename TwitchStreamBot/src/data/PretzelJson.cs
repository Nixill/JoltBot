using System.Text.Json.Nodes;

namespace Nixill.Streaming.JoltBot.Data;

public static class PretzelJson
{
  static JsonObject _root;
  public static JsonObject Root
  {
    get =>
      _root ??= Load();
  }

  public static JsonObject Load()
    => _root = (JsonObject)JsonNode.Parse(File.ReadAllText("ext-data/pretzel/pretzel.json"));

  public static bool IsPlaying => (bool?)Root["player"]["playing"] ?? false;

  public static string Title => (string)Root["track"]["title"];
  public static string Credit => (string)Root["track"]["artistsString"];
  public static IEnumerable<string> Artists => ((JsonArray)Root["track"]["artists"]).Select(n => (string)n);
  public static string Release => (string)Root["track"]["release"]["title"];

  public static bool IsLiked => (bool)Root["track"]["liked"];
}