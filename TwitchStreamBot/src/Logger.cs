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

// file static class Logger
// {
//   const LogLevel MinimumConsoleLogLevel = LogLevel.Information;
//   const LogLevel MinimumFileLogLevel = LogLevel.Debug;

//   static StreamWriter Output = null;
//   static LocalDate Today = default(LocalDate);

//   static readonly DateTimeZone LocalZone = DateTimeZoneProviders.Bcl.GetSystemDefault();
//   static readonly LocalDatePattern DatePattern = LocalDatePattern.Iso;
//   static readonly ZonedDateTimePattern TimePattern = ZonedDateTimePattern.CreateWithInvariantCulture("HH:mm:ss.ttt", null);

//   static ZonedDateTime Now => SystemClock.Instance.GetCurrentInstant()
//     .InZone(LocalZone);

//   static StreamWriter GetCurrentLog() => GetCurrentLog(Now);
//   static StreamWriter GetCurrentLog(Instant now) => GetCurrentLog(now.InZone(LocalZone));
//   static StreamWriter GetCurrentLog(ZonedDateTime now)
//   {
//     if (now.Date == Today) return Output;

//     if (Output != null) Output.Close();
//     Today = now.Date;
//     Output = new StreamWriter($"logs/{DatePattern.Format(Today)}.log");
//     return Output;
//   }

//   internal static void Log(string info, LogLevel level)
//   {
//     var now = Now;
//     var text = $"[{TimePattern.Format(now)}] [{level}] {info}";

//     if (level >= MinimumConsoleLogLevel)
//       Console.WriteLine(text);

//     if (level >= MinimumFileLogLevel)
//     {
//       var file = GetCurrentLog(now);
//       file.WriteLine(text);
//       file.Flush();
//     }
//   }
// }
