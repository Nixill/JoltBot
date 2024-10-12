using Nixill.Collections.Grid.CSV;
using Nixill.Colors;
using Nixill.Utils;

namespace Nixill.Streaming.JoltBot;

public static class Serializers
{
  public static Color DeserializeColor(string input)
    => Color.FromRGBA(input);

  public static string SerializeColor(Color input)
    => input.ToRGBHex();

  public static Dictionary<Type, Func<string, object>> AddArrayDeserializer(
    this Dictionary<Type, Func<string, object>> dict, string separator)
  {
    dict[typeof(string[])] = (str) => str.Split(separator).ToArray();
    return dict;
  }

  public static Dictionary<Type, Func<object, string>> AddArraySerializer(
    this Dictionary<Type, Func<object, string>> dict, string separator)
  {
    dict[typeof(string[])] = lst => ((string[])lst).SJoin(separator);
    return dict;
  }
}