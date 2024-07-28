using System.Text.Json.Nodes;
using Nixill.Utils;
using NodaTime;
using NodaTime.Text;

namespace Nixill.Streaming.JoltBot.JSON;

public static class MemoryJson
{
  static JsonObject _root;
  public static JsonObject Root => _root ?? (_root = (JsonObject)JsonNode.Parse(File.ReadAllText("data/memory.json")));

  public static void Save()
  {
    File.WriteAllText(Root.ToString(), "data/memory.json");
  }

  public static class EmoteMode
  {
    public static Instant ExpiresAt
    {
      get
      {
        var result = InstantPattern.General.Parse((string)Root.ReadPath("emoteMode", "expiresAt"));
        if (result.Success) return result.Value;
        else return Instant.MinValue;
      }
      set
      {
        Root.WritePath(InstantPattern.General.Format(value), "emoteMode", "expiresAt");
        Save();
      }
    }
  }

  public static class Stopper
  {
    public static (
      Instant LastChange,
      string Title,
      string Category,
      string[] Tags
    ) AllInfo
    {
      get => (LastChanged, Title, Category, Tags);
      set
      {
        Root["streamStopper"] = new JsonObject
        {
          ["lastChanged"] = InstantPattern.General.Format(value.LastChange),
          ["title"] = value.Title,
          ["category"] = value.Category,
          ["tags"] = new JsonArray(value.Tags.Select(t => (JsonNode)t).ToArray())
        };
        Save();
      }
    }
    public static Instant LastChanged
    {
      get
      {
        var result = InstantPattern.General.Parse((string)Root.ReadPath("streamStopper", "lastChanged"));
        if (result.Success) return result.Value;
        else return Instant.MinValue;
      }
      set
      {
        Root.WritePath(InstantPattern.General.Format(value), "streamStopper", "lastChanged");
        Save();
      }
    }

    public static string Title
    {
      get => (string)Root.ReadPath("streamStopper", "title");
      set
      {
        Root.WritePath(value, "streamStopper", "title");
        Save();
      }
    }

    public static string Category
    {
      get => (string)Root.ReadPath("streamStopper", "category");
      set
      {
        Root.WritePath(value, "streamStopper", "category");
        Save();
      }
    }

    public static string[] Tags
    {
      get => ((JsonArray)Root.ReadPath("streamStopper", "tags")).Select(n => (string)n).ToArray();
      set
      {
        Root.WritePath(new JsonArray(value.Select(x => (JsonNode)x).ToArray()), "streamStopper", "tags");
        Save();
      }
    }

    public static Dictionary<string, string[]> Aliases = ((JsonObject)Root)
      .Where(kvp => kvp.Value.GetValueKind() == System.Text.Json.JsonValueKind.Array)
      .Select(kvp => (
        kvp.Key,
        ((JsonArray)kvp.Value).Select(
          n => n.ToString()
        ).ToArray()
      )).ToDictionary();

    public static string[] GameTitlesToIgnore = ((JsonObject)Root)
      .Where(kvp => kvp.Value.GetValueKind() == System.Text.Json.JsonValueKind.False)
      .Select(kvp => kvp.Key)
      .ToArray();
  }
}