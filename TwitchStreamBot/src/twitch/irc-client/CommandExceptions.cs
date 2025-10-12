using System.Reflection;

namespace Nixill.Streaming.JoltBot.Twitch;

public class NoDeserializerException(Type t) : Exception($"There is no deserializer for the type {t.Name}")
{
  public readonly Type AttemptedType = t;
}

public class IllegalDeserializerException(MethodInfo m, string message) : Exception($"The method {m} is an invalid deserializer: {message}")
{
  public MethodInfo Method = m;
}

public class IllegalCommandException(MethodInfo m, string message) : Exception($"The method {m} is an invalid command: {message}")
{
  public readonly MethodInfo Method = m;
}

public class NoValueException(string param) : Exception($"No values remaining for parameter {param}")
{
  public readonly string Parameter = param;
}

public class InvalidDeserializeException : Exception
{
  public readonly Type AttemptedType;
  public readonly string HumanReadableType;
  public readonly string Value;
  public readonly string CustomMessage;

  public InvalidDeserializeException(Type type, string hrType, string value)
  {
    AttemptedType = type;
    HumanReadableType = hrType;
    Value = value;
  }

  public InvalidDeserializeException(Type type, string hrType, string value, string message) : base(message)
  {
    AttemptedType = type;
    HumanReadableType = hrType;
    Value = value;
    CustomMessage = message;
  }

  public InvalidDeserializeException(Type type, string hrType, string value, string message, Exception ex) : base(message, ex)
  {
    AttemptedType = type;
    HumanReadableType = hrType;
    Value = value;
    CustomMessage = message;
  }
}

public class UserInputException(string message) : Exception($"User input exception: {message}")
{ }
