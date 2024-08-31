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

# games.csv
All fields except `gameName` are optional.
```csv
gameName,color,aliases,ignoreTitle
My Amazing Game,b42b42,Amazing Game,false
```

# memory.json
```json
{
  "streamStopper": {
    "lastChanged": "2000-00-00T00:00:00Z",
    "title": "Stream title goes here!",
    "category": "Just Chatting",
    "tags": ["Furry", "BotDeveloper", "English"]
  }
}
```

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
  },
  "rewards": {
    "RewardName": "01234567-89ab-cdef-0123-456789abcdef"
  }
}
```

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

# SuperHexagon/attempts.csv
This file may be made publicly accessible in the future. The example row is an actual row of the data.

```csv
attempt_num,redemption_num,score,highlight,notes
109,84,326:59,https://www.twitch.tv/videos/1857653340,All-time PB of Hexagon difficulty
```

# SuperHexagon/redemptions.csv
This file may be made publicly accessible in the future. The example row is an actual row of the data.

```csv
redemption_id,date,redeemer,redeemer_id,level
84,2023-06-21,LevelUpLeo,28552907,Hexagon
```

# ext-data/pretzel.json
This is the last known file format of the Pretzel Rocks json output.

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