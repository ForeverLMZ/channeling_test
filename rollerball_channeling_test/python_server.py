import socket
import struct
from simple_rl_agent import SimpleRLAgent, train
import torch
from rollerball_pb2 import Observation, Action, RewardSignal

# Initialize the RL agent
agent = SimpleRLAgent(4, 128, 2)
optimizer = torch.optim.Adam(agent.parameters(), lr=0.01)

# Socket setup
host = 'localhost'
port = 12345
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind((host, port))
server_socket.listen(1)

print(f"Listening on {host}:{port}")

while True:
    connection, address = server_socket.accept()
    print(f"Connection from {address}")

    # Receive data
    data = connection.recv(1024)
    observation = Observation()
    observation.ParseFromString(data)

    # Process observation and get action
    observation_tensor = torch.tensor([observation.position_x, observation.position_y, observation.velocity_x, observation.velocity_y], dtype=torch.float32)
    action_probs, _ = agent(observation_tensor)
    action = torch.distributions.Categorical(action_probs).sample()

    # Send action back to Unity
    action_msg = Action()
    action_msg.force_x = action[0].item()
    action_msg.force_y = action[1].item()
    connection.send(action_msg.SerializeToString())

    # Receive reward signal
    data = connection.recv(1024)
    reward_signal = RewardSignal()
    reward_signal.ParseFromString(data)

    # Train the agent
    train(agent, optimizer, [(observation_tensor, action, reward_signal.reward)])

    connection.close()
