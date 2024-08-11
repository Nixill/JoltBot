The json files hidden from this folder follow the following format:

aliases.json:
```json
{
  "Game Name on Twitch" [
    "Shortened Name"
  ]
}
```

discord.json:
```json
{
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
    "lastChanged": "2000-00-00T00:00:00.000000000Z",
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
  }
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