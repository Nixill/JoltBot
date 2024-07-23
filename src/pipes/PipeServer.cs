using System.IO.Pipes;
using System.Text.Json.Nodes;

namespace Nixill.Streaming.JoltBot.Pipes;

public static class PipeServer
{
  public static event EventHandler<PipeMessage> MessageReceived;

  public static void ListenForMessages()
  {
    while (true)
    {
      NamedPipeServerStream server = new NamedPipeServerStream("NixJoltBot", PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.CurrentUserOnly);

      server.WaitForConnection();

      StreamReader reader = new StreamReader(server);

      string data = reader.ReadToEnd();
      JsonObject obj = (JsonObject)JsonNode.Parse(data);
      PipeMessage msg = new PipeMessage
      {
        Data = obj
      };

      Task _ = Task.Run(() => MessageReceived.Invoke(null, msg));

      reader.Dispose();
      server.Dispose();
    }
  }
}

public class PipeMessage : EventArgs
{
  public JsonObject Data { get; init; }
}