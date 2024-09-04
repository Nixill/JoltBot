Jolt is a custom-built livestream bot for NixillShadosFox's Twitch streams. It is all of the following working together in harmony:
- A Twitch IRC websocket chatbot, API client, and event consumer using [TwitchLib](https://github.com/TwitchLib/TwitchLib)
- An OBS Websocket client using [OBSWS](https://github.com/Nixill/CSharp.Nixill.OBSWS)
- A named pipe server to receive commands sent from Stream Deck via [a small client](https://github.com/Nixill/DataToJoltBot)
- A Discord webhook client via [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus)

It is coded in C# as a console app. While the source code is available here, it is not in a state anywhere near "plug-and-play" for other setups. Pieces of it may be made available for general use in the future, however.

# Commands
If you're looking for a list of commands usable in Nixill's streams, click [here](https://github.com/Nixill/JoltBot/blob/main/TwitchStreamBot/src/twitch/irc-client/commands/README.md).

# Code of interest
The parts of the code that I think are the most interesting (and are most likely to receive separate releases for general use) are:
- the [End Screen Manager](https://github.com/Nixill/JoltBot/blob/main/TwitchStreamBot/src/obs/EndScreenManager.cs), which updates a list of upcoming streams as well as automatically showing an upcoming raid target
- the [Twitch Command Dispatcher](https://github.com/Nixill/JoltBot/blob/main/TwitchStreamBot/src/twitch/irc-client/CommandDispatch.cs), which takes command input from a Twitch chat message and resolves it to a method with parsed parameters.
- the [Twitch Reward Dispatcher](https://github.com/Nixill/JoltBot/blob/main/TwitchStreamBot/src/twitch/event-client/JoltRewardDispatch.cs), which takes rewards defined with a name in code and turns them into on-site channel points rewards, as well as dispatching events to methods as necessary.

# Third-party libraries
Jolt makes use of the following open-source libraries not linked above:
- [Nixill](https://github.com/Nixill/CSharp.Nixill), a utilities library that you'd *never* guess I made! 😄
- [Ical.Net](https://github.com/rianjs/ical.net) to read my calendar for the end stream manager.
- [NodaTime](https://github.com/nodatime/nodatime), because I'm a Java expat and Joda/Noda handle time classes *so much better* than Microsoft does.
- [NReco.Logging.File](https://github.com/nreco/logging), because I lost *days* trying to make my own combined file-and-console logger and not even succeeding.
- [Quartz](https://github.com/quartznet/quartznet) for scheduling tasks like Super Hexagon Break timers.
- [Websocket.Client](https://github.com/Marfusios/websocket-client), because while I'm gonna make my own OBS library from scratch, I did at least want the thread-help that this gives.