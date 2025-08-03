The pipe runner supports the following calls. Place the desired call as the first command-line arg, and any parameters as further args. For example, for an `Ad.Start` call, the parameters should be `Ad.Start 180`.

# `Ad.Start`
Starts a one-minute countdown to an advertisement.

Parameters:
- First parameter is the length of the ad in seconds, which must be 30, 60, 90, 120, 150, or 180. Other numbers will be clamped to these values and reduced to the next-nearest below. Additionally, the number will be reduced if a shorter ad break would maximize pre-roll-free time.

Remarks: If a countdown is already in progress, it is not restarted, but the time for which the ads should run is updated.


# `Ad.Stop`
Stops a running countdown to an advertisement. (Cannot stop an ad break already in progress!)


# `Commands.Run`
Runs a command as if it was posted in chat (but without actually posting the command to chat).

Parameters: All remaining parameters are interpreted as the command text which should be interpreted by the command dispatcher. Excludes the initial `!`, but includes the command name and any parameters.


# `Markers.Place`
Places a stream and recording marker.


# `Rewards.Refresh`
Refreshes channel point reward eligibility.


# `Scenes.Switch`
Uses the Advanced Scene Switcher functionality to switch to a specific scene with a specific element (or more) showing.

Parameters:
1. Which scene to activate.
2. (all remaining params) Which sources within that scene to show, hiding the rest (out of an arbitrarily selected set of elements on that scene, stored in the OBS data JSON).


# `Screenshots.Save`
Saves a screenshot of active sources, and sends it over discord.

Parameters and options:
- `-f (format)` or `--format (format)`: If specified, the format in which the screenshot should be saved. If not specified, the format is png.
- `-j` or `--jpg` or `--jpeg`: If specified, saves the screenshot in jpg format.
- `--gameSources`: If set, ignore the sources specified in this command and take a screenshot of all game sources.
- `--activeScene`: If set, ignore the sources specified in this command and take a screenshot of the active scene.
- `--previewScene`: If set, ignore the sources specified in this command and take a screenshot of the preview scene. Causes nothing at all to happen if Studio Mode is not enabled.
- `--`: If included, all options after this are interpreted as source names.
- Anything else: A source for which a screenshot should be saved. Ignored if `--gameSources`, `--activeScene`, or `--previewScene` is specified.


# `Upcoming.Read`
Reads upcoming streams from the calendar on file and writes them to the local json file.

Parameters:
- First parameter: The date, in `yyyy-MM-dd` format, to get streams relative to (for testing only!).


# `Upcoming.Write`
Writes upcoming streams from the local json file to the stream scene.