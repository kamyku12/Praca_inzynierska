import torch
import torch.nn as nn

class SelfDrivingNetwork(nn.Module):

    def __init__(self, input_size, hidden1_size, hidden2_size, num_classes):
        super().__init__()
        self.layer1 = nn.Linear(input_size, hidden1_size)
        self.relu1 = nn.ReLU()
        self.layer2 = nn.Linear(hidden1_size, hidden2_size)
        self.relu2 = nn.ReLU()
        self.output_layer = nn.Linear(hidden2_size, num_classes)

    def forward(self, x):
        out = self.relu1(self.layer1(x))
        out = self.relu2(self.layer2(out))
        out = self.output_layer(out)
        return out
    
device = (
    "cuda"
    if torch.cuda.is_available()
    else "cpu"
)