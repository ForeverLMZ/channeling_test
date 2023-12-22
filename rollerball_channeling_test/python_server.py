import socket
import struct
from simple_rl_agent import SimpleRLAgent, train, collect_episode_data
import torch
import signal

num_episodes = 1
# Initialize the RL agent
input_size = 6  # Depends on your observation space
hidden_size = 128
output_size = 2  # Depends on your action space
agent = SimpleRLAgent(input_size, hidden_size, output_size)
optimizer = torch.optim.Adam(agent.parameters(), lr=0.01)

# Socket setup
host = '192.168.0.157'
port = 4242
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind((host, port))
server_socket.listen(1)
print("Server listening...")

def signal_handler(sig, frame):
    print("Ctrl+C detected. Stopping gracefully.")
    server_socket.close()
    exit(0)


# Register the signal handler for Ctrl+C
signal.signal(signal.SIGINT, signal_handler)

'''
# Start the server and listen for connections
current_episode = 0
try:
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
except KeyboardInterrupt:
    print("Ctrl+C detected. Stopping QwQ...")
'''
# Persistent connection outside the episode loop
connection, address = server_socket.accept()
print("Connection accepted")

try:
    for current_episode in range(num_episodes):
        collect_episode_data(connection)  # Adjusted for episode end signal
        current_episode += 1
        print(f"Episode #{current_episode} completed·······································································")
except Exception as e:
    print(f"An error occurred: {e}")
finally:
    print("Closing connection")
    connection.close()
# After training, close the server.
server_socket.close()