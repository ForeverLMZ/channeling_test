using UnityEngine;
using System.Net.Sockets;
using Google.Protobuf;
using Rollerball;
using System.Net;
using System.IO;
using System;



public class RollerBallAgent : MonoBehaviour
{
    public Rigidbody rb;
    public Transform target;
    public float reachDistance = 1.5f; // Distance to consider the target reached
    public Vector3 spawnArea = new Vector3(5, 0.5f, 5); // Area to spawn the target
    private TcpClient client;
    private NetworkStream stream;
    private BinaryReader reader;
    private BinaryWriter writer;
    private const string host = "localhost";
    private const int port = 4242;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ConnectToServer();
        OnEpisodeBegin();

    }

    void ConnectToServer()
    {
        client = new TcpClient(host, port);
        stream = client.GetStream();
        reader = new BinaryReader(stream);
        writer = new BinaryWriter(stream);
    }
    
    void FixedUpdate() //Note to self: This is a Unity-specific function >.<
    {
        if (IsEpisodeComplete())
        {
            OnEpisodeBegin();
        }

        SendObservationToServer();
        ReceiveActionFromServer();
    }
    
    bool IsEpisodeComplete()
    {
        // Check if the agent reached the target or fell off the platform
        if (Vector3.Distance(transform.localPosition, target.localPosition) < reachDistance)
        {
            // Reached the target
            Debug.Log("Episode Complete.");
            return true;
        }
        if (transform.localPosition.y < 0)
        {
            // Fell off the platform
            Debug.Log("Episode Complete.");
            return true;
        }
        return false;
    }
    
    void OnEpisodeBegin()
    {
        Debug.Log("Episode Begins...");
        // Reset agent and target positions
        transform.localPosition = new Vector3(0, 0.5f, 0); // Reset agent position
        rb.velocity = Vector3.zero; // Reset velocity
        // Move the target to a random position within the spawn area
        target.localPosition = new Vector3(UnityEngine.Random.Range(-spawnArea.x, spawnArea.x), 0.5f, UnityEngine.Random.Range(-spawnArea.z, spawnArea.z));
    }
    
    void SendObservationToServer()
    {
        try
        {
            Observation observation = new Observation
            {
                Position = new Vector3Data { X = transform.localPosition.x, Y = transform.localPosition.y, Z = transform.localPosition.z },
                TargetPosition = new Vector3Data { X = target.localPosition.x, Y = target.localPosition.y, Z = target.localPosition.z }
            };

            byte[] serializedObservation = observation.ToByteArray();
            writer.Write(IPAddress.HostToNetworkOrder(serializedObservation.Length));
            writer.Write(serializedObservation);
            Debug.Log("Send Observation To Server Complete");
        }
        catch (Exception e)
        {
            System.Console.WriteLine(e.Message);
            throw;
        }
        
    }
    
    void ReceiveActionFromServer()
    {
        try
        {
            int messageLength = IPAddress.NetworkToHostOrder(reader.ReadInt32());
            byte[] message = reader.ReadBytes(messageLength);
            Rollerball.Action action = Rollerball.Action.Parser.ParseFrom(message);

            ApplyAction(action);
            Debug.Log("Received Action From Server");
        }
        catch (Exception e)
        {
            System.Console.WriteLine(e.Message);
            throw;
        }
        
    }
    
    
    void ApplyAction(Rollerball.Action action)
    {
        // Apply the action received from the server
        Vector3 force = new Vector3(action.Force.X, action.Force.Y, action.Force.Z);
        rb.AddForce(force);
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
