using UnityEngine;
using Google.Protobuf; // Use the Google.Protobuf namespace
using System.IO;
using Stimulus; // The namespace for your generated classes

public class ProtobufTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Create an instance of Command
        Command testCommand = new Command { Id = 1, Type = "Test" };

        // Serialize the Command instance to a byte array
        byte[] data;
        using (MemoryStream stream = new MemoryStream())
        {
            testCommand.WriteTo(stream);
            data = stream.ToArray();
        }

        // Deserialize the byte array back to a Command instance
        Command deserializedCommand;
        using (MemoryStream stream = new MemoryStream(data))
        {
            deserializedCommand = Command.Parser.ParseFrom(stream);
        }

        // Log the deserialized data
        Debug.Log($"Deserialized Data - Id: {deserializedCommand.Id}, Type: {deserializedCommand.Type}");
    }
}
