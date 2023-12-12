import torch
import torch.nn as nn
import torch.optim as optim
import torch.nn.functional as F
import random

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

#TODO: hyperparameter tuning?
input_size = 4  # TODO: what should input_size be?
hidden_size = 128
output_size = 2  # TODO: what should output_size be?

# Initialize the network and optimizer
net = SimpleRLAgent(input_size, hidden_size, output_size)
optimizer = optim.Adam(net.parameters(), lr=0.01)

# Simple Policy Gradient
def train(agent, optimizer, episode_data):
    optimizer.zero_grad()
    cumulative_reward = 0
    for step_data in reversed(episode_data):
        observation, action, reward = step_data
        cumulative_reward = reward + 0.99 * cumulative_reward
        action_prob = agent(observation).gather(1, action.view(-1, 1))
        loss = -torch.log(action_prob) * cumulative_reward
        loss.backward()
    optimizer.step()

# TODO: collect data from Unity via protobuf
def collect_episode_data():
    episode_data = []
    for _ in range(100):  # number of steps in an episode
        observation = torch.randn(4)  
        action = random.randint(0, 1)  
        reward = random.random() 
        episode_data.append((observation, torch.tensor([action]), reward))
    return episode_data

# Training Loop
for episode in range(1000):  # Example number of episodes
    episode_data = collect_episode_data()
    train(net, optimizer, episode_data)
    if episode % 100 == 0:
        print(f"Episode {episode} completed")
