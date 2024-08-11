The json files hidden from this folder (and `ext-data`) follow the following format:

games.json:
```json
{
  "Game Name on Twitch": [
    "Shortened Name"
  ]
}
```

discord.json:
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

memory.json:
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

obs.json:
```json
{
  "server": {
    "ip": "123.45.67.89",
    "port": 1234,
    "password": "abcdefghijklmnopqrstuvwxyz"
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

twitch.json:
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

upcoming.json:
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

ext-data/pretzel.json:
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