using Nixill.Collections;

namespace Nixill.Streaming.JoltBot.Data.UFO50;

public static class UFO50BingosyncCsv
{
  static readonly CSVObjectDictionary<string, UFO50Color> Backgrounds = CSVObjectDictionary.ParseObjectsFromFile("data/ufo50/bingosync.csv", UFO50Color.Parse);

  public static UFO50Color GetColor(string name) => Backgrounds[name.ToLower()];
}

public readonly record struct UFO50Color(string Name, string MainColor, string DarkColor, string LightColor)
{
  public static KeyValuePair<string, UFO50Color> Parse(IDictionary<string, string> input)
    => new(input["color_name"],
      new(input["color_name"], input["hex_code"], input["dark_hex_code"], input["light_hex_code"])
    );
}