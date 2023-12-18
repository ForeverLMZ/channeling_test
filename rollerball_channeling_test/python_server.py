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
port = 4242
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind((host, port))
server_socket.listen(1)

print(f"Listening on {host}:{port}")

def receive_message(connection):
    lengthbuf = connection.recv(4)
    length, = struct.unpack('!I', lengthbuf)
    return connection.recv(length)


while True:
    connection, address = server_socket.accept()
    print(f"Connection from {address}")

    try:
        while True:
            data = receive_message(connection)
            if not data:
                print("no data detected :(")
            # Receive data
            observation = Observation()
            observation.ParseFromString(data)

            # Process the observation to create an input tensor for the agent
            # Assume the observation includes position and target position （Note that we need xyz info instead of only xy)
            obs_tensor = torch.tensor([observation.Position.X, observation.Position.Y, observation.Position.Z,
                                       observation.TargetPosition.X, observation.TargetPosition.Y, observation.TargetPosition.Z],
                                      dtype=torch.float32)

            # Get action from the agent
            agent_output = agent(obs_tensor)
            action_values = agent_output.detach().numpy()  # Convert to numpy array

            # Create an Action message to send back
            action = Action()
            action.ForceX, action.ForceZ = action_values[0], action_values[1]  # Assuming 2D force
            serialized_action = action.SerializeToString()
            connection.sendall(struct.pack('!I', len(serialized_action)))
            connection.sendall(serialized_action)
    finally:
        connection.close()