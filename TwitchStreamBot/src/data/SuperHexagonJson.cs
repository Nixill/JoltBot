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

  public static Instant LastActive
  {
    get => InstantPattern.General.Parse((string)Root["lastActive"]).Value;
    set => Root["lastActive"] = InstantPattern.General.Format(value);
  }

  public static string LastRedeemID
  {
    get => (string)Root["lastRedeemId"];
    set => Root["lastRedeemId"] = value;
  }

  public static string LastRedeemerID
  {
    get => (string)Root["lastRedeemerId"];
    set => Root["lastRedeemerId"] = value;
  }

  public static string LastRedeemerUsername
  {
    get => (string)Root["lastRedeemerUsername"];
    set => Root["lastRedeemerUsername"] = value;
  }

  public static int RedeemNum
  {
    get => (int)Root["redeemNum"];
    set => Root["redeemNum"] = value;
  }

  public static bool RedeemPosted
  {
    get => (bool)Root["redeemPosted"];
    set => Root["redeemPosted"] = value;
  }

  public static SuperHexagonScore RedeemScore
  {
    get => new SuperHexagonScore { Frames = (int)Root["redeemScore"] };
    set => Root["redeemScore"] = value.Frames;
  }

  public static LocalDate StreamDate
  {
    get => LocalDatePattern.Iso.Parse((string)Root["streamDate"]).Value;
    set => Root["streamDate"] = LocalDatePattern.Iso.Format(value);
  }

  public static SuperHexagonLevel Level
  {
    get => Enum.Parse<SuperHexagonLevel>((string)Root["level"]);
    set => Root["level"] = value.ToString();
  }

  public static SuperHexagonStatus Status
  {
    get => Enum.Parse<SuperHexagonStatus>((string)Root["status"]);
    set => Root["status"] = value.ToString();
  }

  public static SuperHexagonLevel[] Played
  {
    get => ((JsonArray)Root["played"]).Select(i => Enum.Parse<SuperHexagonLevel>((string)i)).ToArray();
    set => Root["played"] = new JsonArray(value.Select(i => (JsonNode)i.ToString()).ToArray());
  }
}