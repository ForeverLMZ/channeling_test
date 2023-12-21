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
    private const string host = "192.168.0.157";
    private const int port = 4242;
    
    private Vector3 previousPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ConnectToServer();
        OnEpisodeBegin();
        previousPosition = transform.localPosition;

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
            //Debug.Log("Episode Complete detected in FixedUpdate()");
            SendRewardSignal(true);
            OnEpisodeBegin();
        }

        if (stream != null)
        {
            //Debug.Log("entering into SendObservationToServer() ");
            SendObservationToServer();
            //Debug.Log("entering into ReceiveActionFromServer()");
            ReceiveActionFromServer();
            //Debug.Log("entering into SendRewardSignal()");
            SendRewardSignal(false);
        }
        else
        {
            Debug.Log(stream);
        }
        

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
        Debug.Log("Sending new Observation To Server..");
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
            Debug.Log("Received Action From Server: Fx: "+action.Force.X + "; Fy: "+ action.Force.Y + "; Fz: "+action.Force.Z);
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
    
    void SendRewardSignal(bool done)
    {
        RewardSignal rewardSignal = new RewardSignal
        {
            Done = done,
            // Set the reward value as needed
            Reward = CalculateReward() 
        };

        byte[] serializedRewardSignal = rewardSignal.ToByteArray();
        writer.Write(IPAddress.HostToNetworkOrder(serializedRewardSignal.Length));
        writer.Write(serializedRewardSignal);
    }
    
    float CalculateReward()
    {
        // Constants
        float rewardForReachingTarget = 10.0f;  // Large reward for reaching the target
        float rewardForMovingCloser = 0.1f;     // Small reward for moving closer to the target
        float penaltyForMovingAway = -0.1f;     // Small penalty for moving away from the target

        // Calculate the current and previous distances to the target
        float currentDistance = Vector3.Distance(transform.localPosition, target.localPosition);
        float previousDistance = Vector3.Distance(previousPosition, target.localPosition);

        // Update the previous position for the next frame
        previousPosition = transform.localPosition;

        // Check if the target is reached
        if (currentDistance < reachDistance)
        {
            return rewardForReachingTarget;  // Large reward for reaching the target
        }

        // Reward for moving closer, penalty for moving away
        if (currentDistance < previousDistance)
        {
            return rewardForMovingCloser;  // Reward for getting closer
        }
        else
        {
            return penaltyForMovingAway;  // Penalty for moving away
        }
    }


}
