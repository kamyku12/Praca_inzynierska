import itertools
import numpy as np

# Acceleration actions:
# -1 - full reverse
# 0 - no acceleration
# 1 - full forward
# 0.25 step
acceleration = np.arange(-1, 1, 0.1)

# Rotation actions:
# -1 - go left
# 0 - no rotation
# 1 - go right
# 0.25 step
rotation = np.arange(-1, 1, 0.1)

# Brake actions:
# 0 - no brake
# 1 - brake
brake = [0, 1]

def getActionList():
    return list(itertools.product(acceleration, rotation, brake))