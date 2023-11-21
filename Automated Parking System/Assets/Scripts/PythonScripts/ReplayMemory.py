import random

import numpy as np


class ReplayMemory:

    # N - capacity of ReplayMemory
    def __init__(self, N, batch_size):
        self.capacity = N
        self.memory = []
        self.steps = 0
        self.batch_size = batch_size

    def sample(self):
        sample_memory = random.choices(self.memory, 
                                       k=self.batch_size if len(self.memory) >= self.batch_size else len(self.memory))
        return sample_memory

    def push(self, experience):
        if len(self.memory) >= self.capacity:
            self.memory.pop(0)
            self.memory.append(experience)
        else:
            self.memory.append(experience)

    def reset_memory(self):
        self.memory = []
