using Nixill.Streaming.JoltBot.JSON;
using Nixill.Streaming.JoltBot.Twitch;
using Nixill.Streaming.JoltBot.Twitch.Api;

public static class EventCondition
{
  public static KeyValuePair<string, string> Broadcaster => new("broadcaster_user_id", TwitchJson.Channel.UserId);
  public static KeyValuePair<string, string> Moderator => new("moderator_user_id", TwitchJson.Channel.UserId);
  public static KeyValuePair<string, string> FromBroadcaster => new("from_broadcaster_user_id", TwitchJson.Channel.UserId);
  public static KeyValuePair<string, string> ToBroadcaster => new("to_broadcaster_user_id", TwitchJson.Channel.UserId);
}