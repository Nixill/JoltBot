using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Nixill.Streaming.JoltBot.Discord;
using Nixill.Streaming.JoltBot.OBS;
using Nixill.Streaming.JoltBot.Pipes;
using Nixill.Streaming.JoltBot.Scheduled;
using Nixill.Streaming.JoltBot.Twitch;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace Nixill.Streaming.JoltBot;

class JoltMain
{
  static ILogger Logger = Log.Factory.CreateLogger(typeof(JoltMain));

  static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

  static async Task MainAsync()
  {
    Logger.LogInformation("Jolt server initializing.");
    var twitchSetupTask = JoltTwitchMain.SetUpTwitchConnections();
    var obsSetupTask = JoltOBSClient.SetUp();
    // var discordSetupTask = WebhookClient.SetUp();

    PipeRunner.SetUp();
    ScheduledActions.RunAll();

    await twitchSetupTask;
    await obsSetupTask;
    // await discordSetupTask;

    await MiscStartupActions();

    await Task.Delay(-1);
  }

  static async Task MiscStartupActions()
  {
    await StreamStopper.CheckUpdatedOnStartup();
  }
}
