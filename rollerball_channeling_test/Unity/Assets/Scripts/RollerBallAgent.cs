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
    public Vector3 spawnArea = new Vector3(4.5f, 0.5f, 4.5f); // Area to spawn the target
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
            //OnEpisodeBegin();
        }

        else if (stream != null)
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
            Debug.LogError("Episode Complete.");
            return true;
        }
        if (transform.localPosition.y < 0.5)
        {
            // Fell off the platform
            Debug.LogError("Episode Complete.");
            return true;
        }
        return false;
    }
    
    void OnEpisodeBegin()
    {
        Debug.Log("Episode Begins...");
        // Reset agent and target positions
        transform.position = new Vector3(0, 0.5f, 0); // Reset agent position
        rb.velocity = Vector3.zero; // Reset velocity
        // Move the target to a random position within the spawn area
        target.position = new Vector3(UnityEngine.Random.Range(-spawnArea.x, spawnArea.x), 0.5f, UnityEngine.Random.Range(-spawnArea.z, spawnArea.z));
    }
    
    void SendObservationToServer()
    {
        
        try
        {
            Observation observation = new Observation
            {
                Position = new Vector3Data { X = transform.position.x, Y = transform.position.y, Z = transform.position.z },
                TargetPosition = new Vector3Data { X = target.position.x, Y = target.position.y, Z = target.position.z }
            };

            byte[] serializedObservation = observation.ToByteArray();
            writer.Write(IPAddress.HostToNetworkOrder(serializedObservation.Length));
            writer.Write(serializedObservation);
            Debug.Log("Sent new Observation To Server: ");
            
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
        if (client != null)
        {
            SendRewardSignal(true);  // Ensure server is notified
            client.Close();
        }
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
        
        if (done)
        {
            // Wait for acknowledgment from the server
            WaitForAcknowledgment();
        }
    }
    
    void WaitForAcknowledgment()
    {
        // Set a timeout duration
        float timeout = 5.0f; // 5 seconds for timeout
        float startTime = Time.time;

        // Block until acknowledgment is received or timeout
        while (!stream.DataAvailable)
        {
            if (Time.time - startTime > timeout)
            {
                Debug.LogError("Timeout waiting for acknowledgment from server.");
                // Handle timeout scenario, such as retrying or handling a dropped connection
                break;
            }
        }

        // If the acknowledgment is available, read and handle it
        if (stream.DataAvailable)
        {
            int messageLength = IPAddress.NetworkToHostOrder(reader.ReadInt32());
            byte[] message = reader.ReadBytes(messageLength);
            // Handle the acknowledgment message as needed
            Debug.Log("Acknowledgment received from server.");
        }
    }


    float CalculateReward()
    {
        // Constants
        float rewardForReachingTarget = 10.0f;  // Large reward for reaching the target
        float rewardForMovingCloser = 0.3f;     // Small reward for moving closer to the target
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