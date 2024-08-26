using Microsoft.Extensions.Logging;
using Nixill.Collections.Grid.CSV;
using Nixill.Streaming.JoltBot.Discord;
using Nixill.Streaming.JoltBot.Files;
using Nixill.Streaming.JoltBot.OBS;
using Nixill.Streaming.JoltBot.Pipes;
using Nixill.Streaming.JoltBot.Scheduled;
using Nixill.Streaming.JoltBot.Twitch;

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
    var discordSetupTask = WebhookClient.SetUp();

    DataTableCSVParser.AddDeserializers([typeof(Serializers)]);
    DataTableCSVParser.AddSerializers([typeof(Serializers)]);
    PipeRunner.SetUp();
    ScheduledActions.RunAll();

    await twitchSetupTask;
    await obsSetupTask;
    await discordSetupTask;

    await PretzelFileWatcher.SetUp();

    await MiscStartupActions();

    await Task.Delay(-1);
  }

  static async Task MiscStartupActions()
  {
    await StreamStopper.CheckUpdatedOnStartup();
    await EndScreenManager.UpdateOnStartup();
  }
}
