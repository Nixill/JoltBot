The pipe runner supports the following calls. Place the desired call in the `request` option of the JSON. Other parameters stay in the top level. For example, for an `Ad.Start` call, the entirety of the json should look like this:

```json
{
  "request": "Ad.Start",
  "length": 180
}
```

# `Ad.Start`
Starts a one-minute countdown to an advertisement.

Parameters:
- `length` (int): The length of the ad in seconds, which must be 30, 60, 90, 120, 150, or 180. Other numbers will be clamped to these values and reduced to the next-nearest below. Additionally, the number will be reduced if a shorter ad break would maximize pre-roll-free time.


# `Ad.Stop`
Stops a running countdown to an advertisement. (Cannot stop an ad break already in progress!)


# `Commands.Run`
Runs a command as if it was posted in chat (but without actually posting the command to chat).

Parameters:
- `commandText` (string): The command text that should be interpreted by the command dispatcher. Excludes the initial `!`, but includes the command name and any parameters.


# `Markers.Place`
Places a stream and recording marker.


# `Scenes.Switch`
Uses the Advanced Scene Switcher functionality to switch to a specific scene with a specific element (or more) showing.

Parameters:
- `scene` (string): Which scene to activate
- `show` (string[]): Which sources within that scene to show, hiding the rest (out of an arbitrarily selected set of elements on that scene)


# `Screenshots.Save`
Saves a screenshot of active sources, and sends it over discord.

Parameters:
- `format` (string?): The format in which the screenshot should be saved. Defaults to png.
- `source` (string?): Which source specifically should be saved? If both `source` and `special` are left empty, defaults to `"special": "gameSources"`. If both are specified, `source` prevails.
- `special` (string?): Which source specifically should be saved? Defaults to `gameSources`, has no effect if `source` is specified, and has the following meanings:
  - `gameSources`: Any sources which are active out of an arbitrary list of "game sources" defined within the code.
  - `activeScene`: The entire active scene.
  - `previewScene`: If OBS Studio is in Studio Mode, the entire preview scene. Otherwise, nothing happens.


# `Upcoming.Read`
Reads upcoming streams from the calendar on file and writes them to the local json file.

Parameters:
- `date` (string in `yyyy-MM-dd` format): The date to get streams relative to (for testing only!).


# `Upcoming.Write`
Writes upcoming streams from the local json file to the stream scene.