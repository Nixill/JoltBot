using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Nixill.Streaming.JoltBot.Twitch;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace Nixill.Streaming.JoltBot;

class JoltMain
{
  static ILogger Logger = Log.Factory.CreateLogger("JoltMain");

  static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

  static async Task MainAsync()
  {
    Logger.LogInformation("Jolt server initializing.");
    var setupTask = TwitchMain.SetUpTwitchConnections();

    await Task.Delay(-1);
  }
}
