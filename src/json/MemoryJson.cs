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
}