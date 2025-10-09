using Nixill.Collections;

namespace Nixill.Streaming.JoltBot.Data.UFO50;

public static class UFO50BackgroundsCsv
{
  static readonly CSVObjectDictionary<string, UFO50Background> Backgrounds = CSVObjectDictionary.ParseObjectsFromFile("data/ufo50/backgrounds.csv", UFO50Background.Parse);

  public static UFO50Background GetBackground(string name) => Backgrounds[name.ToUpper()];
}

public readonly record struct UFO50Background(string Name, string UIColor, string ScreenColor)
{
  public static KeyValuePair<string, UFO50Background> Parse(IDictionary<string, string> input)
    => new(input["bg_name"],
      new(input["bg_name"], input["ui_color"], input["screen_color"])
    );
}