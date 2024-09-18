using System.Text.Json.Nodes;

namespace Nixill.Streaming.JoltBot.Data;

public static class RewardsJson
{
  static JsonObject _root;
  public static JsonObject Root
  {
    get => _root ??= (JsonObject)JsonNode.Parse(File.ReadAllText("data/twitch-rewards.json"));
  }

  public static readonly Dictionary<string, string> RewardKeys = Root
    .Select(kvp => (kvp.Key, (string)kvp.Value)).ToDictionary();
  public static readonly Dictionary<string, string> RewardsByKey = Root
    .Select(kvp => ((string)kvp.Value, kvp.Key)).ToDictionary();


  public static void AddReward(string name, string uuid)
  {
    RewardKeys[name] = uuid;
    RewardsByKey[uuid] = name;
    Root[name] = uuid;
  }
}