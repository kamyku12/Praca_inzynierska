import itertools

# Acceleration actions:
# -1 - reverse
# 0 - no acceleration
# 1 - forward
acceleration = [-1, 0, 1]

# Rotation actions:
# -1 - go left
# 0 - no rotation
# 1 - go right
rotation = [-1, 0 ,1]

# Brake actions:
# 0 - no brake
# 1 - brake
brake = [0]

def getActionList():
    return list(itertools.product(acceleration, rotation, brake))