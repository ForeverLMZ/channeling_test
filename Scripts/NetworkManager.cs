using UnityEngine;
using System.Net.Sockets;
using UnityEngine.UI;
using System;
using System.IO;
using Google.Protobuf;
using Stimulus;

public class NetworkManager : MonoBehaviour
{
    public Text numberDisplay;
    public int RequestRandomNumber()
    {

        using (TcpClient client = new TcpClient("192.168.0.118", 4242))
        using (NetworkStream stream = client.GetStream())
        {
            Command requestCommand = new Command { Id = 1, Type = "RequestRandomNumber" };
            requestCommand.WriteTo(stream);

            // Wait for a response with a timeout
            var timeoutDuration = TimeSpan.FromSeconds(5); // 5 seconds timeout
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            while (!stream.DataAvailable)
            {
                if (stopWatch.Elapsed > timeoutDuration)
                {
                    Debug.LogError("Timeout waiting for data from the server");
                }

                System.Threading.Thread.Sleep(10); // Sleep a bit to prevent a tight loop
            }

            // Now read the data
            if (stream.DataAvailable)
            {
                Debug.Log("Signal received: Data available from server");
                //Command responseCommand = Command.Parser.ParseDelimitedFrom(stream);
                
                // Read the length prefix
                byte[] lengthBuffer = new byte[4];
                stream.Read(lengthBuffer, 0, 4);
                int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

                // Now read the actual message
                byte[] messageBuffer = new byte[messageLength];
                stream.Read(messageBuffer, 0, messageLength);

                // Deserialize the Protobuf message
                Command responseCommand = Command.Parser.ParseFrom(messageBuffer);
                // Process the responseCommand as needed
                
                Debug.Log($"{responseCommand}");
                return responseCommand.Number;

            }
            else
            {
                Debug.LogError("No data available from server");
                return -1;
            }

        }
    }
}