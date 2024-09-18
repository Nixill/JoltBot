The files hidden from this folder (and `ext-data`) follow the following formats:

# discord.json
```json
{
  "ownerID": 1234567890,
  "webhooks": {
    "WebhookName": {
      "channelID": 1234567890,
      "channelSecret": "abcdefghijklmnopqrstuvwxyz"
    }
  }
}
```

- `ownerID`: My Discord user ID.
- `webhooks`: A collection of objects describing Discord webhooks.
  - *Key:* The internal name of a webhook.
  - *Value:*
    - `channelID`: Actually a slight misnomer — this is the ID of the *webhook*, not of the channel it posts to.
    - `channelSecret`: Also a slight misnomer — this is the secret hash of the webhook, not related to the channel.

# games.csv
All fields except `gameName` are optional.
```csv
gameName,color,aliases,ignoreTitle
My Amazing Game,b42b42,Amazing Game,false
```

- `gameName`: The name of a game, as written on Twitch. Compared case-insensitive, but should be entered the same as on Twitch.
- `color`: The hex code of a color which should be applied to the interface during this game.
- `aliases`: A semicolon-separated list of aliases; any of these being present in the stream title will be considered a match for putting the game name in it.
- `ignoreTitle`: If `true`, the stream title is ignored entirely during this game.

# memory.json
```json
{
  "streamClock": {
    "lastKnownTime": "2024-09-01T04:27:00Z",
    "lastKnownState": false,
    "lastStartTime": "2024-08-25T22:25:16Z",
    "lastEndTime": "2024-08-25T22:47:33Z"
  },
  "streamStopper": {
    "lastChanged": "2000-00-00T00:00:00Z",
    "title": "Stream title goes here!",
    "category": "Just Chatting",
    "tags": ["Furry", "BotDeveloper", "English"]
  }
}
```

- `streamClock`: A ticking clock on-stream.
  - `lastKnownTime`: The last time this file was saved.
  - `lastKnownState`: Whether the stream was online (`true`) or offline (`false`) when this file was last saved.
  - `lastStartTime`: The last time the bot saw the stream be started. If the stream is online at bot startup but was offline when the file was last saved, this is set to the current time.
  - `lastEndTime`: The last time the bot saw the stream be started. If the stream is offline at bot startup but was online when the file was last saved, this is set to `lastKnownTime`.
- `streamStopper`: Memories controlling the automatic "you didn't change the title and I'm not letting you stream until you do" mechanism.
  - `lastChanged`: The last time the bot saw this info changed. On startup, if any of this info doesn't match what the bot last saw, this is updated to the current time.
  - `title`: The stream title as of the last time the bot saw it changed.
  - `category`: The stream game/category as of the last time the bot saw it changed.
  - `tags`: The stream tags as of the last time the bot saw it changed.

# obs.json
```json
{
  "server": {
    "ip": "123.45.67.89",
    "port": 1234,
    "password": "abcdefghijklmnopqrstuvwxyz"
  },
  "bottomText": {
    "sc_SceneName": 36,
  },
  "sceneSwitcher": {
    "sc_SceneName": [
      "itm_ItemToHide1",
      "itm_ItemToHide2"
    ]
  },
  "screenshotFolder": "C:\\Path\\To\\Screenshot\\Folder"
}
```

- `server`: Details about the OBS websocket server
  - `ip`: Its local IP
  - `port`: The port to use for connecting
  - `password`: The password to use for authentication
- `bottomText`: Information used for running the text-ticker at the bottom of OBS scenes
  - *Key:* The name of a scene that includes the text-ticker.
  - *Value:* The length of text that can be displayed from the ticker in that scene.
- `sceneSwitcher`: Information used for running the advanced scene switcher.
  - *Key:* The name of a scene supported by the advanced scene switcher.
  - *Value:* An array:
    - *Item:* One of the scene items that will be hidden unless explicitly called out for showing in the scene switch command.
- `screenshotFolder`: The folder in which screenshots taken from stream should be stored.

# twitch.json
```json
{
  "id": "abcdefghijklmnopqrstuvwxyz",
  "secret": "abcdefghijklmnopqrstuvwxyz",
  "bot": {
    "name": "Username",
    "token": "abcdefghijklmnopqrstuvwxyz",
    "refresh": "abcdefghijklmnopqrstuvwxyz",
    "uid": "1234567890"
  },
  "channel": {
    "name": "Username",
    "token": "abcdefghijklmnopqrstuvwxyz",
    "refresh": "abcdefghijklmnopqrstuvwxyz",
    "uid": "1234567890"
  }
}
```

- `id`: The application client ID for Jolt.
- `secret`: The client secret for Jolt.
- `bot` and `client`: Respectively the user info for the bot user and the streamer for Jolt.
  - `name`: The username of the account.
  - `token`: An access token for the account.
  - `refresh`: The refresh token to create a new access token for the account.
  - `uid`: The user ID for the account.

# twitch-rewards.json
```json
{
  "RewardName": "01234567-89ab-cdef-0123-456789abcdef"
}
```

Information on Twitch Channel Points Rewards managed by Jolt.
- *Key:* The name for that reward as used in source code.
- *Value:* The UUID for that reward as stored on Twitch.

# upcoming.json
```json
{
  "calendarLink": "https://example.com/path/to/calendar.ics",
  "gameIconFolder": "C:\\Path\\to\\Game\\Icon\\Folder",
  "upcoming": [
    {
      "date": "2019-09-19",
      "name": "First Stream!",
      "game": "Hot Lava",
      "channel": "https://twitch.tv/NixillShadowFox",
      "andMore": false
    },    
    {
      "date": "2019-09-21",
      "name": "Second Stream!",
      "game": "Wizard of Legend",
      "channel": "https://twitch.tv/NixillShadowFox",
      "andMore": true
    }
  ]
}
```

- `calendarLink`: A link to the ~~past~~ calendar of upcoming streams. Should be an ICS file.
- `gameIconFolder`: A folder containing game images for the ending screen. (It's named game *icon* folder because this previously was just icons only.)
- `upcoming`: An array of upcoming streams.
  - *Item:*
    - `date`: The date of the upcoming stream.
    - `name`: The stream name.
    - `game`: The stream game.
    - `channel`: Where is this happening? If it doesn't match `ownChannel`, this'll be shown on screen.
    - `andMore`: Are more streams happening on the same day?
- `ownChannel`: A link to your own channel.

# SuperHexagon/attempts.csv
This file may be made publicly accessible in the future. The example row is an actual row of the data.

```csv
attempt_id,redemption_id,score,highlight,notes
109,84,326:59,https://www.twitch.tv/videos/1857653340,All-time PB of Hexagon difficulty
```

- `attempt_id`: Which attempt was it? (Autoincrement)
- `redemption_id`: Which redemption was the attempt a part of?
- `score`: What was the final time of the attempt? If it's > 60:00, it's considered a win.
- `highlight`: Some runs have highlights! Those are linked here.
- `notes`: Some runs have notes! Those are written here.

# SuperHexagon/redemptions.csv
This file may be made publicly accessible in the future. The example row is an actual row of the data.

```csv
redemption_id,date,redeemer,redeemer_id,level
84,2023-06-21,LevelUpLeo,28552907,Hexagon
```

- `redemption_id`: Which redemption was it? (Autoincrement)
- `date`: Which stream was it? This date matches the start of the stream even if calendar date rolls over during the stream.
- `redeemer`: Which username redeemed this break?
- `redeemer_id`: Which user ID redeemed this break? Is empty if unknown, because this wasn't tracked for two years and a couple accounts no longer exist.
- `level`: Which level was played? Should always be exactly one of the following strings:
  - `Hexagon`
  - `Hexagoner`
  - `Hexagonest`
  - `HyperHexagon`
  - `HyperHexagoner`
  - `HyperHexagonest`

# SuperHexagon/status.json

```json
{
  "lastActive": "2024-09-04T05:09:00Z",
  "lastRedeem": "01234567-89ab-cdef-0123-456789abcdef",
  "redeemNum": 237,
  "streamDate": "2024-09-03",
  "status": "Hexagoner",
  "played": [
    "Hexagon",
    "HyperHexagon",
    "Hexagoner"
  ]
}
```

- `lastActive`: The last time the Super Hexagon Break was active, which means any of the following occurred:
  - The channel points reward was redeemed.
  - A score was entered for a redemption (whether or not this closes the redemption).
  - One hour passed with an open redemption but no activity.
- `lastRedeem`: The ID of the most recent Super Hexagon Break redemption, if unfulfilled. Is automatically fulfilled when a redemption ends, or refunded if an hour has passed with no activity.
- `redeemNum`: Which redemption number is active (used for filling `attempts.csv`)?
- `streamDate`: What date, in the time zone specified in `memory.json`, did the current stream start?
- `status`: Exactly one of the Super Hexagon levels defined below.
- `played`: Which Super Hexagon levels have been played? Includes the one currently being played, if applicable. Possible values listed below, except "None".

Super Hexagon levels:
- `None`
- `Hexagon`
- `Hexagoner`
- `Hexagonest`
- `HyperHexagon`
- `HyperHexagoner`
- `HyperHexagonest`

# ext-data/pretzel.json
This is the last known file format of the Pretzel Rocks json output. Note that it's written in the file as collapsed JSON, not formatted JSON.

```json
{
  "track": {
    "title": "My Cool Music Track",
    "artistsString": "TwistBit & Nixill",
    "artists": [
      {
        "name": "TwistBit"
      },
      {
        "name": "Nixill"
      }
    ],
    "release": {
      "title": "My Cool Album"
    },
    "liked": true
  },
  "player": {
    "playing": true
  }
}
```

- `track`: Information about the track being played.
  - `title`: The track's title.
  - `artistsString`: A single string denoting all artists.
  - `artists`: An array of artists.
    - *Item:* A single artist object.
      - `name`: That artist's name.
  - `release`: Information about the album.
    - `title`: The title of the album.
  - `liked`: If the song was liked in the player or not.
- `player`: Information about the player itself.
  - `playing`: Whether the player is playing (`true`) or paused (`false`).