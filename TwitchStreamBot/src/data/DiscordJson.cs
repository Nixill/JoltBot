using System.Text.Json.Nodes;
using Nixill.Utils;
using NodaTime;
using NodaTime.Text;

namespace Nixill.Streaming.JoltBot.Data;

public static class DiscordJson
{
  static JsonObject _root;
  public static JsonObject Root => _root ?? (_root = (JsonObject)JsonNode.Parse(File.ReadAllText("data/discord.json")));

  public static void Save()
  {
    File.WriteAllText(Root.ToString(), "data/discord.json");
  }

  public static Dictionary<string, (ulong ID, string Secret)> Webhooks = ((JsonObject)Root["webhooks"])
    .Select(kvp => (kvp.Key, ((ulong)kvp.Value["channelID"], (string)kvp.Value["channelSecret"])))
    .ToDictionary();

  public static readonly ulong OwnerID = (ulong)Root["ownerID"];
}