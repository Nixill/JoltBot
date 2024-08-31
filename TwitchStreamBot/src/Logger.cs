using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Text;
using NReco.Logging.File;

namespace Nixill.Streaming.JoltBot;

public static class Log
{
  // public static void Trace(string info, params object[] args) => Local.LogTrace(info, args);
  // public static void Debug(string info, params object[] args) => Logger.Log(info, LogLevel.Debug);
  // public static void Info(string info, params object[] args) => Logger.Log(info, LogLevel.Information);
  // public static void Warn(string info, params object[] args) => Logger.Log(info, LogLevel.Warning);
  // public static void Error(string info, params object[] args) => Logger.Log(info, LogLevel.Error);
  // public static void Critical(string info, params object[] args) => Logger.Log(info, LogLevel.Critical);

  public static readonly ILoggerFactory Factory = LoggerFactory.Create(builder => builder.AddFile("logs/{0:yyyy}-{0:MM}-{0:dd}.log",
      (FileLoggerOptions opts) =>
      {
        opts.Append = true;
        opts.FormatLogFileName = name => string.Format(name, DateTime.UtcNow);
        opts.MinLevel = LogLevel.Trace;
      }).AddConsole()
      // .SetMinimumLevel(LogLevel.Trace)
      );

  // private static readonly ILogger Local = Factory.CreateLogger("BasicLogger");
}
