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

    public void RequestRandomNumber()
    {
        try
        {
            using (TcpClient client = new TcpClient("192.168.0.118", 4242))
            using (NetworkStream stream = client.GetStream())
            {
                Command requestCommand = new Command { Id = 1, Type = "RequestRandomNumber" };
                requestCommand.WriteTo(stream);

                // Additional error handling for data reception
                try
                {
                    // Wait for a response with a timeout
                    var timeoutDuration = TimeSpan.FromSeconds(5);  // 5 seconds timeout
                    var stopWatch = System.Diagnostics.Stopwatch.StartNew();
                    while (!stream.DataAvailable)
                    {
                        if (stopWatch.Elapsed > timeoutDuration)
                        {
                            Debug.LogError("Timeout waiting for data from the server");
                            return;
                        }
                        System.Threading.Thread.Sleep(10); // Sleep a bit to prevent a tight loop
                    }
                    // Now read the data
                    if (stream.DataAvailable)
                    {
                        Debug.Log("Signal received: Data available from server");
                        //Command responseCommand = Command.Parser.ParseDelimitedFrom(stream);
                        Command responseCommand = Command.Parser.ParseFrom(stream);
                        if (responseCommand != null)
                        {
                            numberDisplay.text = "Received number: " + responseCommand.Number;
                        }
                        else
                        {
                            Debug.LogError("Received null Command object from server");
                        }
                    }
                    else
                    {
                        Debug.LogError("No data available from server");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Error in receiving or processing data: " + e.Message);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("NetworkManager encountered an error: " + e.Message);
        }
    }
}