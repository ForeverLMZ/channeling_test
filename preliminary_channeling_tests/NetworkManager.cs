using UnityEngine;
using System.Net.Sockets;
using UnityEngine.UI;
using System;
using System.IO;
using Google.Protobuf;
using Stimulus;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour
{
    public Text numberDisplay;
    public RawImage imageDisplay; 
    private readonly Queue<Action> _executeOnMainThread = new Queue<Action>();

    public void ExecuteOnMainThread(Action action)
    {
        if (action == null)
        {
            Debug.Log("No action to execute on main thread!");
            return;
        }

        lock (_executeOnMainThread)
        {
            _executeOnMainThread.Enqueue(action);
        }
    }

    void Update()
    {
        
        while (_executeOnMainThread.Count > 0)
        {
            Action action = null;

            lock (_executeOnMainThread)
            {
                if (_executeOnMainThread.Count > 0)
                {
                    action = _executeOnMainThread.Dequeue();
                }
            }

            action?.Invoke();
        }
    }

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

    public void RequestImage()
    {
        byte[] imageData = null; // Declare imageData here so it's accessible later
        using (TcpClient client = new TcpClient("192.168.0.118", 4242))
        using (NetworkStream stream = client.GetStream())
        {
            Command requestCommand = new Command { Id = 2, Type = "RequestRandomPicture" };
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

                // Read the size of the image first
                // Read the size of the image first
                byte[] sizeInfo = new byte[4];
                int bytesRead = stream.Read(sizeInfo, 0, 4);
                if (bytesRead == 4)
                {
                    // If the Python server is using 'big' endian, you should reverse the array before converting
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(sizeInfo);

                    int imageSize = BitConverter.ToInt32(sizeInfo, 0);

                    // Now make sure that you're reading as many bytes as imageSize states
                    imageData = new byte[imageSize];
                    int totalBytesRead = 0;
                    while (totalBytesRead < imageSize)
                    {
                        int read = stream.Read(imageData, totalBytesRead, imageSize - totalBytesRead);
                        if (read == 0)
                        {
                            // Handle the end of the stream (the server closed the connection)
                            throw new Exception("The server closed the connection.");
                        }

                        totalBytesRead += read;
                    }

                    // Assuming the rest of your code follows
                }
                else
                {
                    // Handle the case where the size info was not fully read
                    Debug.LogError("Did not receive full size information for the image.");
                    return; // Add a return statement here to exit the method early.

                }

                // Use the imageData in the main thread
                ExecuteOnMainThread(() => {
                    Debug.Log($"Executing on main thread. Image data null or empty? {imageData == null || imageData.Length == 0}");
                    if (imageData != null && imageData.Length > 0)
                    {
                        Texture2D texture = new Texture2D(2, 2);
                        bool isLoaded = texture.LoadImage(imageData); // Load image on the main thread
                        if (isLoaded)
                        {
                            if (imageDisplay != null)
                            {
                                Debug.Log("Texture loaded, setting to imageDisplay.");
                                imageDisplay.texture = texture;
                            }
                            else
                            {
                                Debug.LogError("imageDisplay is not assigned.");
                            }
                        }
                        else
                        {
                            Debug.LogError("Failed to load texture from image data.");
                        }
                    }
                    else
                    {
                        Debug.LogError("Image data is null or empty.");
                    }
                });
            }
            else
            {
                Debug.LogError("No data available from server.");
            }
        }
    }
}