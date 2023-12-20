using UnityEngine;
using System.Net.Sockets;
using Google.Protobuf; 
using Rollerball;
using System;
using System.Net;
using System.Threading.Tasks;

public class CameraCapture : MonoBehaviour
{
    public RenderTexture renderTexture;
    private Texture2D texture2D;
    private const string host = "192.168.0.157";
    private const int port = 4242;
    private TcpClient client;
    private NetworkStream stream;

    async void ConnectToServer()
    {
        try
        {
            client = new TcpClient();
            await client.ConnectAsync(host, port);
            stream = client.GetStream();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error connecting to server: {ex.Message}");
        }
    }

    void Start()
    {
        Debug.Log("Pixel script started..");
        ConnectToServer();
        OnEpisodeBegin();
        texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
    }

    void OnEpisodeBegin()
    {
        Debug.Log("Begins to send pixel data...");
    }

    void FixedUpdate() 
    {
        if(client == null || !client.Connected)
        {
            Debug.LogWarning("Client not connected to server.");
            return;
        }

        CaptureAndSendData();
    }

    async void CaptureAndSendData()
    {
        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply(); // Store the pixel data

        // Convert the pixel data to a byte array
        byte[] pixelDataArray = texture2D.GetRawTextureData();

        // Create a new protobuf message and assign the pixel data
        PixelData pixelDataMessage = new PixelData { Data = Google.Protobuf.ByteString.CopyFrom(pixelDataArray) };

        // Serialize the protobuf message to a byte array
        byte[] serializedMessage = pixelDataMessage.ToByteArray();

        // Send the data asynchronously to avoid freezing
        await SendDataAsync(serializedMessage);
    }

    async Task SendDataAsync(byte[] serializedMessage)
    {
        try
        {
            // Convert the length of the message to a byte array in network order
            byte[] lengthPrefix = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(serializedMessage.Length));

            // Asynchronously send the length prefix and the message
            await stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
            await stream.WriteAsync(serializedMessage, 0, serializedMessage.Length);

            Debug.Log("Send Pixel Data To Server Complete");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error sending data: {ex.Message}");
        }
    }

    void OnDestroy()
    {
        // Close the stream and client when the object is destroyed
        if (stream != null)
            stream.Close();
        if (client != null)
            client.Close();
    }
}
