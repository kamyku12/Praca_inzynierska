import torch
import torch.nn as nn

from torch.autograd import Variable

class SelfDrivingNetwork(nn.Module):

    def __init__(self, input_size, hidden1_size, hidden2_size, num_classes):
        super().__init__()
        self.flatten = nn.Flatten()
        self.linear_relu_stack = nn.Sequential(
            nn.Linear(input_size, hidden1_size),
            nn.ReLU(),
            nn.Linear(hidden1_size, hidden2_size),
            nn.ReLU(),
            nn.Linear(hidden2_size, num_classes)
        )

    def forward(self, x):
        x = self.flatten()
        logits = self.linear_relu_stack(x)
        return logits
    
device = (
    "cuda"
    if torch.cuda.is_available()
    else "cpu"
)