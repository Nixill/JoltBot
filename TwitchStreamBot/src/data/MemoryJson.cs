using System.Text.Json.Nodes;
using Nixill.Utils;
using NodaTime;
using NodaTime.Text;

namespace Nixill.Streaming.JoltBot.Data;

public static class MemoryJson
{
  static JsonObject _root;
  public static JsonObject Root => _root ??= (JsonObject)JsonNode.Parse(File.ReadAllText("data/memory.json"));

  public static void Save()
  {
    File.WriteAllText("data/memory.json", Root.ToString());
  }

  public static class Clock
  {
    public static Instant LastKnownTime
    {
      get => InstantPattern.General.Parse((string)Root["streamClock"]["lastKnownTime"]).Value;
      set => Root["streamClock"]["lastKnownTime"] = InstantPattern.General.Format(value);
    }

    public static bool LastKnownState
    {
      get => (bool)Root["streamClock"]["lastKnownState"];
      set => Root["streamClock"]["lastKnownState"] = value;
    }

    public static Instant LastStartTime
    {
      get => InstantPattern.General.Parse((string)Root["streamClock"]["lastStartTime"]).Value;
      set => Root["streamClock"]["lastStartTime"] = InstantPattern.General.Format(value);
    }

    public static Instant LastEndTime
    {
      get => InstantPattern.General.Parse((string)Root["streamClock"]["lastEndTime"]).Value;
      set => Root["streamClock"]["lastEndTime"] = InstantPattern.General.Format(value);
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

    public static Dictionary<string, string[]> Aliases = Root
      .Where(kvp => kvp.Value.GetValueKind() == System.Text.Json.JsonValueKind.Array)
      .Select(kvp => (
        kvp.Key,
        ((JsonArray)kvp.Value).Select(
          n => n.ToString()
        ).ToArray()
      )).ToDictionary();

    public static string[] GameTitlesToIgnore = Root
      .Where(kvp => kvp.Value.GetValueKind() == System.Text.Json.JsonValueKind.False)
      .Select(kvp => kvp.Key)
      .ToArray();
  }
}