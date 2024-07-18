using System.Reflection;

namespace Nixill.Streaming.JoltBot.Twitch;

public class NoDeserializerException : Exception
{
  public readonly Type AttemptedType;

  public NoDeserializerException(Type t) : base($"There is no deserializer for the type {t.Name}")
  {
    AttemptedType = t;
  }
}

public class IllegalDeserializerException : Exception
{
  public MethodInfo Method;

  public IllegalDeserializerException(MethodInfo m, string message) : base($"The method {m} is an invalid deserializer: {message}")
  {
    Method = m;
  }
}

public class IllegalCommandException : Exception
{
  public readonly MethodInfo Method;

  public IllegalCommandException(MethodInfo m, string message) : base($"The method {m} is an invalid command: {message}")
  {
    Method = m;
  }
}

public class NoValueException : Exception
{
  public readonly string Parameter;

  public NoValueException(string param) : base($"No values remaining for parameter {param}")
  {
    Parameter = param;
  }
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
