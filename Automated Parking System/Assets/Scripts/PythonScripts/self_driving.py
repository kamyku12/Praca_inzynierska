import socket
import time
import random
from actionMap import getActionList
from SelfDrivingNetwork import SelfDrivingNetwork
import torch
import torch.nn as nn
import torch.optim as optim
from ReplayMemory import ReplayMemory
import numpy as np
from matplotlib import pyplot

def connect(sock, host, port):
    error = True
    while error:
        try:
            sock.connect((host, port))
            error = False
        except:
            error = True


def initialize(sock):
    received_data_initialize = None
    try:
        sock.sendall("waitingForStart".encode("UTF-8"))
        received_data_initialize = sock.recv(1024).decode("UTF-8")
    except:
        pass

    if received_data_initialize == "start":
        print("Waiting to unpause")
        sock.sendall("started".encode("UTF-8"))
        return False

    return True


def move_car(sock, acceleration, rotation, braking):
    time.sleep(0.025)

    data_to_send = "|".join(map(str, [acceleration, rotation, braking]))
    sock.sendall(data_to_send.encode("UTF-8"))



def handle_received_data(received_data, calculations, state, reward):
    newPaused = False
    newStopped = False
    newEpisode = False
    newCalculations = calculations
    newState = state
    newReward = reward
    
    if received_data == 'stop':
        #print('stop')
        newStopped = True
        newCalculations = True

    if received_data == 'pause':
        #print('pause')
        newPaused = True
        newCalculations = True

    if received_data == 'newEpisode':
        #print('newEpisode')
        newPaused = True
        newEpisode = True

    if received_data == 'unPause':
        #print('unPause')
        newPaused = False
        newCalculations = True

    if '|' in received_data:
        stateValues = received_data.replace(',', '.').split('|')
        velocity = stateValues[0].split(':')
        distance = stateValues[3].split(':')
        velocity_keys = ['x', 'y', 'z']
        distance_keys = ['l_u', 'r_u', 'l_b', 'r_b']

        for (value, key) in zip(velocity, velocity_keys):
            newState[f"velocity_{key}"] = float(value)

        for (value, key) in zip(distance, distance_keys):
            newState[f"distance_{key}"] = float(value)

        newState["rotation"] = float(stateValues[1])
        newState["isCarInsideSpot"] = stateValues[2] == "true"
        newReward = calculate_reward(newState)


    return newPaused, newStopped, newEpisode, newCalculations, newState, newReward

def calculate_reward(state):
    velocity = [state['velocity_x'], state['velocity_y'], state['velocity_z']]
    rotation = [state['rotation']]
    in_spot = state['isCarInsideSpot']
    distances = [state["distance_l_u"], state["distance_r_u"], state["distance_l_b"], state["distance_r_b"]]
    timer = state['timer']

    velocity_coeff = -1.0
    rotation_coeff = -5.0
    in_spot_coeff = 1000.0
    distance_coeff = -2.0
    timer_coeff = -0.1

    velocity_reward = velocity_coeff * np.linalg.norm(velocity)
    rotation_reward = rotation_coeff * np.abs(rotation)
    in_spot_reward = in_spot_coeff if in_spot else 0.0
    distance_reward = distance_coeff * np.sum(distances)
    timer_reward = timer_coeff * timer

    return velocity_reward + rotation_reward + in_spot_reward + distance_reward + timer_reward

def main():
    paused = True
    stopped = False
    initializing = True
    newEpisode = False
    calculations = True
    steps_counter = 0
    rewards_memory = []
    rewards_for_plot = []
    input_data = None

    reward = 0
    state = dict(
        velocity_x = 0,
        velocity_y = 0,
        velocity_z = 0,
        rotation = 0,
        isCarInsideSpot = False,
        distance_l_u = 0,
        distance_r_u = 0,
        distance_l_b = 0,
        distance_r_b = 0,
        timer = 0.0
    )



    host, port = "127.0.0.1", 25001
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.settimeout(3)
    actions = getActionList()
    acceleration, rotation, braking = random.choice(actions)

    # hyperparameters

    # random action epsilon - % of chance to pick random action
    # instead of action chosen by value approximation
    # it is used to introduce exploration into our reinforcement learning
    random_action = 0.8
    random_action_decay = 0.9

    # learning rate of neural network
    learning_rate = 0.1

    # size of replay memory used in learning
    replay_memory_size = 1_000_000

    # after which number of episodes update weights of second neural network
    steps_to_update = 1_000_000

    # ---------------

    experienceMemory = ReplayMemory(replay_memory_size)
    
    model = None
    # If there is a already saved model of neural network load it to the model
    
    # Check if there is a saved model file
    try:
        f = open('SelfDrivingNetwork.pt')
    except FileNotFoundError:
        # Create new model if no file detected
        model = SelfDrivingNetwork(len(state), 128, 128, len(actions))
        print('Creating new model')
    else:
        model = torch.load('./SelfDrivingModel.pt')
        model.eval()
        print('Loaded saved configuration:')
        # print all parameters of model
        for param_tensor in model.state_dict():
            print(param_tensor, "\t", model.state_dict()[param_tensor].size())

    # create target model which will have updated weights after 'episodes_to_update'
    target_model = SelfDrivingNetwork(len(state), 128, 128, len(actions))
    target_model.load_state_dict(model.state_dict())

    criterion = nn.MSELoss()
    optimizer = optim.SGD(model.parameters(), lr=learning_rate)
    
    print("Now start Unity simulator!")
    connect(sock, host, port)

    print("Started listening")
    while not stopped:
        if initializing:
            initializing = initialize(sock)
        else:
            if not paused:
                print(state)
                if random.random() < random_action:
                    acceleration, rotation, braking = random.choice(actions)
                else:
                    # pass state as an array to a model to calculate best action
                    # Extract values from the dictionary
                    input_values = list(state.values())

                    # Convert values to a PyTorch tensor
                    input_data = torch.tensor(input_values, dtype=torch.float32).view(1, -1)

                    # Get value of model
                    value = model(input_data)
                    action = torch.argmax(model(input_data))

                    # gradient descent
                    target = target_model(input_data)
                    loss = criterion(value, target)

                    optimizer.zero_grad()
                    loss.backward()
                    optimizer.step()

                    acceleration, rotation, braking = actions[action]
                    print(f"{acceleration}, {rotation}, {braking}")

                move_car(sock, acceleration, rotation, braking)

                # update steps counter and weights of nn
                # if steps counter reaches max steps
                if steps_counter == steps_to_update:
                    target_model.load_state_dict(model.state_dict())
                    steps_counter = 0
                else:
                    steps_counter += 1

                rewards_memory.append(reward)

            if paused and not newEpisode: 
                sock.sendall("waitingForUnpause".encode("UTF-8"))
                # print('Waiting to unpause')

            if newEpisode:
                if calculations:
                    sock.sendall("newEpisodeCalculations".encode("UTF-8"))
                    print("New episode calculations")
                    
                    # Get average of rewards for plot
                    rewards_for_plot.append(np.average(rewards_memory))
                    rewards_memory = []
                    
                    # update random action probability
                    random_action *= random_action_decay
                    print(random_action)
                    
                    calculations = False
                else:
                    sock.sendall("waitingForUnpause".encode("UTF-8"))

            

            try:
                received_data = sock.recv(1024).decode("UTF-8")
                paused, stopped, newEpisode, calculations, state, reward = handle_received_data(received_data, calculations, state, reward)
            except Exception as e:
                print(e)
                stopped = True
    
    # save model to SelfDrivingModel.pt file to update this model 
    # in another iteration of project
    torch.save(model, 'SelfDrivingModel.pt')

    # plot reward changes
    pyplot.plot(rewards_for_plot)
    pyplot.show()

    

if __name__ == "__main__":
    try:
        ## your code, typically one function call
        main()
    except:
        import sys
        print(sys.exc_info()[0])
        import traceback
        print(traceback.format_exc())
    finally:
        print("Press Enter to continue ...") 
        input()
    