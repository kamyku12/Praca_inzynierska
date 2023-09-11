import itertools

acceleration = [-1, 0, 1]
rotation = [-1, 0 ,1]
brake = [0, 1]

def getActionList():
    return list(itertools.product(acceleration, rotation, brake))