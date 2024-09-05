using System.Text.Json.Nodes;
using Nixill.Streaming.JoltBot.Twitch.Events.Rewards;
using Nixill.Utils;
using NodaTime;
using NodaTime.Text;

namespace Nixill.Streaming.JoltBot.Data;

public static class SuperHexagonJson
{
  static JsonObject _root;
  public static JsonObject Root => _root ??= (JsonObject)JsonNode.Parse(File.ReadAllText("data/SuperHexagon/status.json"));

  public static void Save()
  {
    File.WriteAllText("data/SuperHexagon/status.json", Root.ToString());
  }

  public static Instant LastActivity
  {
    get => InstantPattern.General.Parse((string)Root["lastActive"]).Value;
    set => Root["lastActive"] = InstantPattern.General.Format(value);
  }

  public static string LastRedeem
  {
    get => (string)Root["lastRedeem"];
    set => Root["lastRedeem"] = value;
  }

  public static int RedeemNum
  {
    get => (int)Root["redeemNum"];
    set => Root["redeemNum"] = value;
  }

  public static LocalDate StreamDate
  {
    get => LocalDatePattern.Iso.Parse((string)Root["streamDate"]).Value;
    set => Root["streamDate"] = LocalDatePattern.Iso.Format(value);
  }

  public static SuperHexagonLevel Status
  {
    get => Enum.Parse<SuperHexagonLevel>((string)Root["status"]);
    set => Root["status"] = value.ToString();
  }

  public static SuperHexagonLevel[] Played
  {
    get => ((JsonArray)Root["played"]).Select(i => Enum.Parse<SuperHexagonLevel>((string)i)).ToArray();
    set => Root["played"] = new JsonArray(value.Select(i => (JsonNode)i.ToString()).ToArray());
  }
}