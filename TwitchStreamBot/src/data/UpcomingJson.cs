using System.Text.Json.Nodes;
using NodaTime;
using NodaTime.Text;

namespace Nixill.Streaming.JoltBot.Data;

public static class UpcomingJson
{
  static JsonObject _root;
  public static JsonObject Root => _root ?? (_root = (JsonObject)JsonNode.Parse(File.ReadAllText("data/upcoming.json")));

  public static void Save()
  {
    File.WriteAllText("data/upcoming.json", Root.ToString());
  }

  public static string CalendarLink = (string)Root["calendarLink"];
  public static string GameIconFolder = (string)Root["gameIconFolder"];

  public static UpcomingStream? First
  {
    get
    {
      JsonArray arr = (JsonArray)Root["upcoming"];
      if (arr.Count >= 1) return new UpcomingStream((JsonObject)arr[0]);
      else return null;
    }
    set
    {
      JsonArray arr = (JsonArray)Root["upcoming"];
      if (value == null)
      {
        if (arr.Count >= 1) arr.RemoveAt(0);
      }
      else
      {
        if (arr.Count >= 1) arr[0] = value.Value.ToJson();
        else arr.Add(value.Value.ToJson());
      }
    }
  }

  public static UpcomingStream? Second
  {
    get
    {
      JsonArray arr = (JsonArray)Root["upcoming"];
      if (arr.Count >= 2) return new UpcomingStream((JsonObject)arr[1]);
      else return null;
    }
    set
    {
      JsonArray arr = (JsonArray)Root["upcoming"];
      if (value == null)
      {
        if (arr.Count >= 2) arr.RemoveAt(1);
      }
      else
      {
        if (arr.Count >= 2) arr[1] = value.Value.ToJson();
        else arr.Add(value.Value.ToJson());
      }
    }
  }
}

public struct UpcomingStream
{
  public LocalDate Date;
  public string Name;
  public string Game;
  public string Channel;
  public bool AndMore;

  public UpcomingStream(JsonObject o)
  {
    Date = LocalDatePattern.Iso.Parse((string)o["date"]).Value;
    Name = (string)o["name"];
    Game = (string)o["game"];
    Channel = (string)o["channel"];
    AndMore = (bool)o["andMore"];
  }

  public JsonObject ToJson()
  {
    return new JsonObject
    {
      ["date"] = LocalDatePattern.Iso.Format(Date),
      ["name"] = Name,
      ["game"] = Game,
      ["channel"] = Channel,
      ["andMore"] = AndMore
    };
  }
}