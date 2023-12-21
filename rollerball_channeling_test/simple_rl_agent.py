import torch
import torch.nn as nn
import torch.optim as optim
import torch.nn.functional as F
import struct
import numpy
from rollerball_pb2 import Observation, Action, RewardSignal, PixelData 

GAMMA = 0.99 #discount factor


# Neural Network for the Agent
class SimpleRLAgent(nn.Module):
    def __init__(self, input_size, hidden_size, output_size):
        super(SimpleRLAgent, self).__init__()
        self.fc1 = nn.Linear(input_size, hidden_size)
        self.fc2 = nn.Linear(hidden_size, output_size)

    def forward(self, x):
        x = F.relu(self.fc1(x))
        x = F.softmax(self.fc2(x), dim=-1)
        return x

input_size = 6  
hidden_size = 128
output_size = 2  

# Initialize the network and optimizer
net = SimpleRLAgent(input_size, hidden_size, output_size)
optimizer = optim.Adam(net.parameters(), lr=0.01)

# Simple Policy Gradient
def train(agent, optimizer, episode_data):
    optimizer.zero_grad()
    cumulative_reward = 0
    for step_data in reversed(episode_data):
        observation, action, reward = step_data
        cumulative_reward = reward + GAMMA * cumulative_reward #discounted cumulative reward
        action_prob = agent(observation).gather(1, action.view(-1, 1))
        loss = -torch.log(action_prob) * cumulative_reward
        loss.backward()
    optimizer.step()

'''
# Reads the incoming Protobuf messages from the Unity side.
def receive_message(connection):
    print("starting to receive incoming message...")
    lengthbuf = connection.recv(4)
    print("lengthbuf is", lengthbuf)

    if len(lengthbuf) != 4:
        print(" the message is incomplete or the connection was closed")
        return None
    length, = struct.unpack('!I', lengthbuf)
    return connection.recv(length)
'''

def receive_message(connection):
    lengthbuf = connection.recv(4)
    if len(lengthbuf) != 4:
        raise ValueError("Invalid length received")
    length = struct.unpack('!I', lengthbuf)[0]

    serialized_data = connection.recv(length)
    if len(serialized_data) != length:
        raise ValueError("Did not receive the expected amount of data")

    return serialized_data




#Collect data from Unity via protobuf
def collect_episode_data(connection):
    episode_data = []
    while True:
        print("starting a new update:")
        serilized_observation = receive_message(connection) #this is the XYZ position observation
        if not serilized_observation:
            break  # End of episode
        observation = Observation()
        observation.ParseFromString(serilized_observation)
        print("observation (X,Y,Z) received:", observation.Position.X, observation.Position.Y, observation.Position.Z )
        
        # Convert observation to tensor
        obs_tensor = torch.tensor([observation.Position.X, observation.Position.Y, observation.Position.Z,
                                   observation.TargetPosition.X, observation.TargetPosition.Y, observation.TargetPosition.Z],
                                  dtype=torch.float32)

        '''
        print("receiving pixel data...")
        serilized_pixel = receive_message(connection) #this is the pixel data
        print("serilized pixel data received")
        

        # Deserialize the protobuf message
        pixel_data_message = PixelData.FromString(serilized_pixel)
        # Extract the pixel data byte array
        pixel_data = pixel_data_message.data

        print("pixel data received: ",pixel_data)
        '''

        # Get action from the agent
        agent_output = net(obs_tensor)
        
        action_values = agent_output.detach().numpy()  # Convert to numpy array


        # Choose action based on the agent's output
        action = Action()
        action.Force.X, action.Force.Y, action.Force.Z = action_values[0], 0, action_values[1]  # Set X and Z forces; Y is set to 0
        serialized_action = action.SerializeToString()
        connection.sendall(struct.pack('!I', len(serialized_action)))
        connection.sendall(serialized_action)

        # Receive reward signal from Unity
        #print("Receiving reward signal from Unity")

        reward_data = receive_message(connection)
        reward_signal = RewardSignal()
        reward_signal.ParseFromString(reward_data)
        print("reward signal received:",reward_signal.reward)

        #episode_data.append((obs_tensor, torch.tensor([action_values]), reward_signal.reward))
        episode_data.append((obs_tensor, torch.tensor(numpy.array(action_values)), reward_signal.reward))
        print("episode data appended")

        if reward_signal.done:
            break  # End of episode

    return episode_data

