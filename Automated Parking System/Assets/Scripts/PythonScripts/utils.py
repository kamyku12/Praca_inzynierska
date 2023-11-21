import numpy as np
import torch
import torch.nn as nn
from ReplayMemory import ReplayMemory
from SelfDrivingNetwork import SelfDrivingNetwork
import torch.optim as optim


def calculate_reward(state):
    velocity = [state['velocity_x'], state['velocity_y'], state['velocity_z']]
    rotation = [state['rotation']]
    in_spot = state['isCarInsideSpot']
    distances = [state["distance_l_u"], state["distance_r_u"],
                 state["distance_l_b"], state["distance_r_b"]]
    timer = state['timer']

    velocity_coeff = 1.0
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


def handle_received_data(received_data, calculations, state, reward):
    newPaused = False
    newStopped = False
    newEpisode = False
    newCalculations = calculations
    newState = state
    newReward = reward

    if received_data == 'stop':
        # print('stop')
        newStopped = True
        newCalculations = True

    if received_data == 'pause':
        # print('pause')
        newPaused = True
        newCalculations = True

    if received_data == 'newEpisode':
        # print('newEpisode')
        newPaused = True
        newEpisode = True

    if received_data == 'unPause':
        # print('unPause')
        newPaused = False
        newCalculations = True

    if '|' in received_data:
        stateValues = received_data.replace(',', '.').split('|')
        velocity = stateValues[0].split(':')
        distance = stateValues[4].split(':')
        velocity_keys = ['x', 'y', 'z']
        distance_keys = ['l_u', 'r_u', 'l_b', 'r_b']

        for (value, key) in zip(velocity, velocity_keys):
            newState[f"velocity_{key}"] = float(value)

        for (value, key) in zip(distance, distance_keys):
            newState[f"distance_{key}"] = float(value)

        newState["timer"] = float(stateValues[3])
        newState["rotation"] = float(stateValues[1])
        newState["isCarInsideSpot"] = stateValues[2] == "true"
        newReward = calculate_reward(newState)

    return newPaused, newStopped, newEpisode, newCalculations, newState, newReward


def create_tensor_from_state(state):

    state_tensor = None
    if isinstance(state, list):
        # Extract values for dictionaries
        input_values = [list(d.values()) for d in state]

        # Create list of tensors
        state_tensor = [torch.tensor(i).view(1, -1) for i in input_values]
    else:
        # Extract values from the dictionary
        input_values = list(state.values())

        # Convert values to a PyTorch tensor
        state_tensor = torch.tensor(input_values).view(1, -1)

    return state_tensor


def create_nn_models(state, actions, learning_rate):
    # Check if there is a saved model file
    try:
        f = open('SelfDrivingNetwork.pt')
    except FileNotFoundError:
        # Create new model if no file detected
        model = SelfDrivingNetwork(len(state), 128, 128, len(actions))
        print('Creating new model')
    else:
        model = torch.load('SelfDrivingModel.pth')
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

    return model, target_model, criterion, optimizer


def training(replay_memory: ReplayMemory, criterion, optimizer, model, target_model, discount_factor):
    print('training session')
    sample = replay_memory.sample()
    discount_factor_tensor = torch.tensor(discount_factor)

    current_states = [s[0] for s in sample]
    picked_actions = [s[1] for s in sample]
    rewards = [s[2] for s in sample]
    next_states = [s[3] for s in sample]

    # Value of model neural network
    current_states_tensor = create_tensor_from_state(current_states)
    values = torch.stack([torch.max(model(state))
                          for state in current_states_tensor]).view(-1, 1)

    # Value of target neural network
    next_states_tensor = create_tensor_from_state(next_states)
    target_values = [target_model(state)
                     for state in next_states_tensor]

    tensor_rewards = torch.tensor(np.asarray(rewards))

    y = []
    for idx, state in enumerate(current_states):
        if state["isCarInsideSpot"]:
            y.append(tensor_rewards[idx])
        else:
            y.append(tensor_rewards[idx] +
                     discount_factor_tensor * torch.max(target_values[idx]))

    y = torch.stack(y).to(torch.float32)

    loss = criterion(y, values)

    optimizer.zero_grad()
    loss.backward()
    optimizer.step()
    print('end of training session')
