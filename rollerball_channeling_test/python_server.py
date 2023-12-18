import socket
import struct
from simple_rl_agent import SimpleRLAgent, train, collect_episode_data
import torch

num_episodes = 1000

# Initialize the RL agent
input_size = 6  # Depends on your observation space
hidden_size = 128
output_size = 2  # Depends on your action space
agent = SimpleRLAgent(input_size, hidden_size, output_size)
optimizer = torch.optim.Adam(agent.parameters(), lr=0.01)

# Socket setup
host = 'localhost'
port = 4242
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind((host, port))
server_socket.listen(1)


# Start the server and listen for connections
current_episode = 0
while current_episode < num_episodes:
    connection, address = server_socket.accept()
    print("connection accepted")
    try:
        episode_data = collect_episode_data(connection)
        if episode_data:
            print("episode_data received")
            print(episode_data)
            train(agent, optimizer, episode_data)
            current_episode += 1
            print(f"Episode {current_episode} completed")
    finally:
        print("connection closed")
        connection.close()

# After training, close the server.
server_socket.close()
print("Training completed.")