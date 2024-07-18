using NodaTime;
using NodaTime.Text;
using NodaTime.TimeZones;
using TwitchLib.Client.Events;
using Args = TwitchLib.Client.Events.OnChatCommandReceivedArgs;

namespace Nixill.Streaming.JoltBot.Twitch.Commands;

[CommandContainer]
public static class InfoCommands
{
  [Command("discord")]
  public static Task DiscordCommand(Args ev)
    => ev.ReplyAsync("Join the Shadow Den discord server! → https://discord.nixill.net/");

  [Command("pronouns")]
  public static Task PronounsCommand(Args ev)
    => ev.ReplyAsync("Nixill's pronouns are they/she (or anything except he or it)! If you visit"
      + "https://pronouns.alejo.io/, you can get an extension to view people's pronouns or set them for other users of"
      + "the extension.");

  static DateTimeZone Here = BclDateTimeZone.ForSystemDefault();
  static SystemClock Clock = SystemClock.Instance;
  static Instant Now => Clock.GetCurrentInstant();
  static ZonedDateTimePattern TimePattern = ZonedDateTimePattern.CreateWithInvariantCulture("HH:mm", null);
  static string CurrentTime => TimePattern.Format(Now.InZone(Here));

  [Command("time")]
  public static Task TimeCommand(Args ev)
    => ev.ReplyAsync($"I'm still working out the schedule! It's currently {CurrentTime} here.");
}
