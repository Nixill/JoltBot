using Nixill.Utils.Extensions;

namespace Nixill.Streaming.JoltBot.Twitch;

[DeserializerContainer]
public static class Deserializers
{
  [Deserializer]
  public static string DeserializeString(IList<string> input, bool isLongText)
  {
    if (isLongText)
    {
      string ret = input.StringJoin(" ");
      input.Clear();
      return ret;
    }
    else
    {
      return input.Pop();
    }
  }

  [Deserializer]
  public static int DeserializeInt(IList<string> input, bool isLongText)
  {
    string val = input.Pop();
    try { return int.Parse(val); }
    catch (FormatException) { throw new InvalidDeserializeException(typeof(int), "integer", val); }
    catch (OverflowException) { throw new InvalidDeserializeException(typeof(int), "integer", val, $"(must be in the range {int.MinValue} to {int.MaxValue}, inclusive)"); }
    catch (Exception e) { throw new InvalidDeserializeException(typeof(int), "integer", val, e.Message, e); }
  }

  [Deserializer]
  public static long DeserializeLong(IList<string> input, bool isLongText)
  {
    string val = input.Pop();
    try { return long.Parse(val); }
    catch (FormatException) { throw new InvalidDeserializeException(typeof(long), "integer", val); }
    catch (OverflowException) { throw new InvalidDeserializeException(typeof(long), "integer", val, $"(must be in the range {long.MinValue} to {long.MaxValue}, inclusive)"); }
    catch (Exception e) { throw new InvalidDeserializeException(typeof(long), "integer", val, e.Message, e); }
  }

  [Deserializer]
  public static double DeserializeDouble(IList<string> input, bool isLongText)
  {
    string val = input.Pop();
    try { return double.Parse(val); }
    catch (FormatException) { throw new InvalidDeserializeException(typeof(double), "number", val); }
    catch (OverflowException) { throw new InvalidDeserializeException(typeof(double), "number", val, $"(must be in the range {double.MinValue} to {double.MaxValue}, inclusive)"); }
    catch (Exception e) { throw new InvalidDeserializeException(typeof(double), "number", val, e.Message, e); }
  }

  [Deserializer]
  public static bool DeserializeBool(IList<string> input, bool isLongText)
  {
    string val = input.Pop();
    try { return bool.Parse(val); }
    catch (FormatException) { throw new InvalidDeserializeException(typeof(bool), "boolean", val); }
    catch (Exception e) { throw new InvalidDeserializeException(typeof(bool), "boolean", val, e.Message, e); }
  }
}