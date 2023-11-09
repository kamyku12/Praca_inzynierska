import numpy as np


class ReplayMemory:

    # N - capacity of ReplayMemory
    def __init__(self, N):
        self.capacity = N
        self.memory = []
    
    def sample():
        return np.random.choice(self.memory)     

    def push(experience):
        if len(self.memory) >= self.capacity:
            self.memory.pop(0)
            self.memory.push(experience)
        else:
            self.memory.push(experience)