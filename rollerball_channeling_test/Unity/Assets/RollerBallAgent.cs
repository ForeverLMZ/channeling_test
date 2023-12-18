using UnityEngine;
using System.Net.Sockets;
using System.IO;
using Rollerball;

public class RollerBallAgent : MonoBehaviour
{
    public Rigidbody ballRigidbody;
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
        ballRigidbody = GetComponent<Rigidbody>();
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
    
    void FixedUpdate()
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
        if (Vector3.Distance(transform.position, target.position) < reachDistance)
        {
            // Reached the target
            return true;
        }
        if (transform.position.y < 0)
        {
            // Fell off the platform
            return true;
        }
        return false;
    }
    
    void OnEpisodeBegin()
    {
        // Reset agent and target positions
        transform.position = Vector3.zero; // Reset agent position
        rb.velocity = Vector3.zero; // Reset velocity
        // Move the target to a random position within the spawn area
        target.position = new Vector3(Random.Range(-spawnArea.x, spawnArea.x), 0.5f, Random.Range(-spawnArea.z, spawnArea.z));
    }
    
    void SendObservationToServer()
    {
        Observation observation = new Observation
        {
            Position = new Vector3Data { X = transform.position.x, Y = transform.position.y, Z = transform.position.z },
            TargetPosition = new Vector3Data { X = target.position.x, Y = target.position.y, Z = target.position.z }
        };

        byte[] serializedObservation = observation.ToByteArray();
        writer.Write(IPAddress.HostToNetworkOrder(serializedObservation.Length));
        writer.Write(serializedObservation);
    }
    
    void ReceiveActionFromServer()
    {
        int messageLength = IPAddress.NetworkToHostOrder(reader.ReadInt32());
        byte[] message = reader.ReadBytes(messageLength);
        Action action = Action.Parser.ParseFrom(message);

        ApplyAction(action);
    }
    
    /* (Let's try a more modularized version)
    void FixedUpdate()
    {
        if (client == null || !client.Connected)
        {
            ConnectToServer();
        }

        // Create an Observation instance and populate it with the current data
        Observation observation = new Observation
        {
            PositionX = transform.localPosition.x,
            PositionZ = transform.localPosition.z,
            VelocityX = ballRigidbody.velocity.x,
            VelocityZ = ballRigidbody.velocity.z
        };

        // Serialize and send the observation to the Python server
        byte[] serializedObservation = observation.ToByteArray();
        writer.Write(IPAddress.HostToNetworkOrder(serializedObservation.Length));
        writer.Write(serializedObservation);

        int messageLength = IPAddress.NetworkToHostOrder(reader.ReadInt32());
        byte[] message = reader.ReadBytes(messageLength);
        Action action = Action.Parser.ParseFrom(message);

        ApplyAction(action);
    }

    /*
    private byte[] ObservationToBytes(Observation observation)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (Google.Protobuf.CodedOutputStream outputStream = new Google.Protobuf.CodedOutputStream(memoryStream))
            {
                observation.WriteTo(outputStream);
            }
            return memoryStream.ToArray();
        }
    }
    */
        */

    void ApplyAction(Action action)
    {
        // Apply the action received from the server
        Vector3 force = new Vector3(action.ForceX, 0, action.ForceZ);
        ballRigidbody.AddForce(force);
    }
    
    /*
    private Action BytesToAction(byte[] actionBytes)
    {
        Action action = new Action();
        using (MemoryStream memoryStream = new MemoryStream(actionBytes))
        {
            using (Google.Protobuf.CodedInputStream inputStream = new Google.Protobuf.CodedInputStream(memoryStream))
            {
                action.MergeFrom(inputStream);
            }
        }
        return action;
    }*/


    void OnDestroy()
    {
        // Close the stream and client when the object is destroyed
        if (stream != null)
            stream.Close();
        if (client != null)
            client.Close();
    }
}
