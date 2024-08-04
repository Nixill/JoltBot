The following is a list of chat commands currently implemented in JoltBot:

# For any user
## `!add`
```
!add <number> [number…]
```

Adds one or more integers. Decimal numbers aren't allowed, but negative numbers are.


## `!ban`
```
!ban <noun>
```

Sends a message pretending that the named noun is now banned from chat.

*^(This command does not apply any actual moderation to the target, nor does it even check if the target is a valid Twitch user.)*


## `!coinflip`
```
!coinflip
!coin
```

Flips a coin.


## `!discord`
```
!discord
```

Gives the calling user a link to the discord server.


## `!group`
```
!group
```

Tells the user what group(s) they're in as related to chat commands.


## `!images`
```
!images
```

Gives the calling user a link to a list of "webcam" background images.


## `!multi`
```
!multi
!multistream
```

If other streamers are mentioned in the title, this creates a link to a multistre.am view of all of them and myself.


## `!pronouns`
```
!pronouns
```

Gives the calling user information on my pronouns (they/she, or anything but he or it), and a link to the [pronouns extension](https://pr.alejo.io/)



# For moderators only
## `!closechat`
```
!closechat
```

Closes chat to non-VIPs (to the extent twitch allows) by applying the following settings:
- Emote only mode
- Followers only mode (3 months)
- Slow mode (2 minutes)
- Subscribers only mode

VIPs and moderators are immune to closed-chat settings.


## `!countdown`
```
!countdown
```

Creates a five-second countdown in chat, useful for synchronization.


## `!id`
```
!id <username>
```

Gets the numeric Twitch ID of a certain user.


## `!openchat`
```
!openchat
```

Opens chat to all users by removing the following settings:
- Emote only mode
- Followers only mode
- Slow mode
- Subscribers only mode


## `!shoutout`, `!so`
```
!so <username>
!shoutout <username>
```

Sends a specific user a shoutout, including both a custom message and a Twitch-side `/shoutout`.

Please note: `/shoutout` can only be used once every two minutes, and can only be used towards a specific streamer once every hour. However, the custom message side of a `!so` can be used more frequently.