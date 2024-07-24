using System.Text.Json.Nodes;
using Nixill.Streaming.JoltBot.JSON;
using TwitchLib.EventSub.Websockets.Core.EventArgs.Channel;

namespace Nixill.Streaming.JoltBot.Twitch.Events;

public static class JoltRewards
{
  internal static async Task Redemption(object sender, ChannelPointsCustomRewardRedemptionArgs args)
  {
    var evt = args.Notification.Payload.Event;

    switch (TwitchJson.RewardsByKey.GetValueOrDefault(evt.Reward.Id))
    {
      case null: break;
      case "SuperHexagon": await SuperHexagonAlert(); break;
    }
  }

  internal static async Task SuperHexagonAlert()
  {
    await JoltChatBot.Chat(
      "⚠ Super Hexagon contains flashing and spinning colors that may cause problems for some viewers. "
      + "It will usually last around 3 minutes (Nix's highscore is just under 5½ minutes). ⚠");
    await JoltChatBot.Chat("Previously achieved scores can be viewed here: " +
      "https://docs.google.com/spreadsheets/d/1j6HpNdJHCxhNuHM0JjtIZsG17coXZiN2Xspxo0I_4k0/edit#gid=0");
  }
}