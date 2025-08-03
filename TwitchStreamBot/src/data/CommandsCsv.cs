using System.Text.Json.Nodes;
using Nixill.Collections;

namespace Nixill.Streaming.JoltBot.Data;

public static class CommandsCsv
{
  static readonly CSVObjectDictionary<string, SimpleCommand> Commands = CSVObjectDictionary.ParseObjectsFromFile("data/commands.csv", SimpleCommand.Parse);
  static readonly Dictionary<string, SimpleCommand> TempCommands = [];

  public static void Save()
  {
    Commands.FormatCSVToFile("data/commands.csv", SimpleCommand.UnparseKVP);
  }

  public static SimpleCommand GetCommand(string commandName)
    => TryGetCommand(commandName, out SimpleCommand cmd) ? cmd : null;

  public static bool IsTempCommand(string commandName)
    => TempCommands.ContainsKey(commandName.ToLower());

  public static bool IsPermCommand(string commandName)
    => Commands.ContainsKey(commandName.ToLower());

  public static bool TryGetCommand(string commandName, out SimpleCommand cmd)
    => TempCommands.TryGetValue(commandName.ToLower(), out cmd) || Commands.TryGetValue(commandName.ToLower(), out cmd);

  public static bool AddCommand(string commandName, string response, bool mod, bool title)
  {
    try
    {
      Commands.Add(new(commandName.ToLower(), new SimpleCommand
      {
        CommandName = commandName,
        RequireModerator = mod,
        RequireTitle = title,
        Response = response
      }));
      Save();
      return true;
    }
    catch (Exception)
    {
      return false;
    }
  }

  public static bool AddTempCommand(string commandName, string response, bool mod, bool title)
  {
    try
    {
      TempCommands.Add(commandName.ToLower(), new SimpleCommand
      {
        CommandName = commandName,
        RequireModerator = mod,
        RequireTitle = title,
        Response = response
      });
      return true;
    }
    catch (Exception)
    {
      return false;
    }
  }

  public static bool RemoveCommand(string commandName)
  {
    if (RemoveBeforeSave(commandName))
    {
      Save();
      return true;
    }
    else return false;
  }

  private static bool RemoveBeforeSave(string commandName)
  {
    if (TempCommands.Remove(commandName.ToLower())) return true;
    return Commands.Remove(commandName.ToLower());
  }
}

public class SimpleCommand
{
  public required string CommandName { get; init; }
  public required string Response { get; set; }
  public required bool RequireModerator { get; init; }
  public required bool RequireTitle { get; init; }

  internal static KeyValuePair<string, SimpleCommand> Parse(IDictionary<string, string> dictionary)
    => new(
      dictionary["commandName"].ToLower(),
      new SimpleCommand
      {
        CommandName = dictionary["commandName"].ToLower(),
        Response = dictionary["response"],
        RequireModerator = dictionary["moderator"] == "true",
        RequireTitle = dictionary["title"] == "true"
      }
    );

  internal IDictionary<string, string> Unparse()
    => new Dictionary<string, string>
    {
      ["commandName"] = CommandName,
      ["response"] = Response,
      ["moderator"] = RequireModerator ? "true" : "",
      ["title"] = RequireTitle ? "true" : ""
    };

  internal static IDictionary<string, string> UnparseKVP(KeyValuePair<string, SimpleCommand> kvp)
    => kvp.Value.Unparse();
}