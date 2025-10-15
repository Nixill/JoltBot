using System.Text.Json.Nodes;

namespace Nixill.Streaming.JoltBot.Data.UFO50;

public static class MusicJson
{
  static JsonObject _root;
  public static JsonObject Root => _root ??= (JsonObject)JsonNode.Parse(File.ReadAllText("data/UFO50/music.json"));

  public static List<string> GetAllTracks() =>
    [.. ((JsonObject)Root["trackNames"]).Select(kvp => kvp.Key)];

  public static List<string> GetTracksFor(string game) =>
    [.. ((JsonArray)Root["gameTracks"][game]).Select(n => (string)n)];

  public static List<string> GetTracksFor(string game1, string game2)
  {
    if (game1 == game2) return GetTracksFor(game1);
    else return [.. GetTracksFor(game1), .. GetTracksFor(game2)];
  }

  public static string GetTitleOf(string track) => (string)Root["trackNames"][track];

  public static string SelectMenuTheme()
  {
    JsonArray menuTrackList = (JsonArray)Root["menuMusic"];
    string track = (string)menuTrackList[(int)Random.Shared.NextInt64(menuTrackList.Count)];
    return track;
  }
}