using System.Text.Json.Nodes;

namespace Nixill.Streaming.JoltBot.Data;

public static class TwitchJson
{
  static JsonObject _root;
  public static JsonObject Root
  {
    get =>
      _root ?? (_root = (JsonObject)JsonNode.Parse(File.ReadAllText("data/twitch.json")));
  }

  public static readonly string ID = (string)Root["id"];
  public static readonly string Secret = (string)Root["secret"];

  public static readonly AuthInfo Bot = new AuthInfo(Root["bot"]);
  public static readonly AuthInfo Channel = new AuthInfo(Root["channel"]);

  public static void Save()
  {
    File.WriteAllText("data/twitch.json", Root.ToString());
  }
}

public class AuthInfo
{
  public string Name
  {
    get => (string)Obj["name"];
    set => Obj["name"] = value;
  }
  public string Token
  {
    get => (string)Obj["token"];
    set => Obj["token"] = value;
  }
  public string Refresh
  {
    get => (string)Obj["refresh"];
    set => Obj["refresh"] = value;
  }
  public string UserId
  {
    get => (string)Obj["uid"];
    set => Obj["uid"] = value;
  }
  public string Which => Obj.GetPropertyName();

  JsonObject Obj;

  public AuthInfo(JsonNode input) : this((JsonObject)input) { }
  public AuthInfo(JsonObject input)
  {
    Obj = input;
  }
}
