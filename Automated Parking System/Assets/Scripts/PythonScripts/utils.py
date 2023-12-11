import time
import numpy as np
import torch
import torch.nn as nn
from ReplayMemory import ReplayMemory
from SelfDrivingNetwork import SelfDrivingNetwork
import torch.optim as optim
import os
from numba import jit


def calculate_reward(state):
    velocity_x = state['velocity_x']
    velocity_z = state['velocity_z']
    rotation = state['rotation']
    in_spot = state['isCarInsideSpot']
    distances = [state["f_distance"], state["b_distance"]]
    timer = state['timer']

    # Define individual rewards and penalties
    time_penalty = -0.1 * timer
    rotation_penalty = -0.1 * abs(rotation)
    velocity_penalty = -0.001 * (velocity_x**2 + velocity_z**2)
    # Encourage getting closer to the parking spot
    distance_reward = -0.1 * sum(distances)
    inside_parking_reward = 3.0 if in_spot else 0.0

    # Combine components with weights
    reward = (
        rotation_penalty +
        velocity_penalty +
        distance_reward +
        inside_parking_reward + 
        time_penalty
    )

    return reward


def handle_received_data(received_data, calculations, state, reward):
    newPaused = False
    newStopped = False
    newEpisode = False
    newCalculations = calculations
    newState = state.copy()
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
        velocity_keys = ['x', 'z']

        for (value, key) in zip(velocity, velocity_keys):
            newState[f"velocity_{key}"] = float(value)

        newState["f_distance"] = float(distance[0])
        newState["b_distance"] = float(distance[1])

        newState["timer"] = float(stateValues[3])
        newState["rotation"] = float(stateValues[1])
        newState["isCarInsideSpot"] = stateValues[2] == "True"
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
        model = SelfDrivingNetwork(len(state), 128, 128, len(actions))
        model.load_state_dict(torch.load(
            f"{os.getcwd()}/Assets/Scripts/PythonScripts/SelfDrivingModel.pth"))
        print('Loaded saved configuration:')
        # print all parameters of model
        for param_tensor in model.state_dict():
            print(param_tensor, "\t", model.state_dict()[param_tensor].size())
    except Exception as e:
        # Create new model if no file detected
        model = SelfDrivingNetwork(len(state), 128, 128, len(actions))
        print('Creating new model')

    # create target model which will have updated weights after some number of episodes
    target_model = SelfDrivingNetwork(len(state), 128, 128, len(actions))
    target_model.load_state_dict(model.state_dict())

    criterion = nn.MSELoss()
    optimizer = optim.Adam(model.parameters(), lr=learning_rate)

    return model, target_model, criterion, optimizer


NUMBER_OF_EPOCHS = 50


def is_terminating(state):
    return state["isCarInsideSpot"] and state["f_distance"] == 0 and state["b_distance"] == 0 and state["rotation"] == 0,


def loss_in_epoch(model, criterion, states, y) -> torch.Tensor:

    # Value of model neural network
    values = torch.stack([model(create_tensor_from_state(s[0]))
                          for s in states]).view(1, -1)

    loss = criterion(values, y)

    return loss


def training(replay_memory: ReplayMemory, criterion, optimizer, model, target_model, discount_factor):
    print('training session')
    loss_in_session = []
    for i in range(NUMBER_OF_EPOCHS):
        print(f"epoch number: {i}")
        sample = replay_memory.sample()
        discount_factor_tensor = torch.tensor(discount_factor)
        current_states = [s[0] for s in sample]

        # Value of target neural network
        target_values = torch.stack(
            [target_model(create_tensor_from_state(s[2])) for s in sample])

        # Recorded rewards
        tensor_rewards = torch.tensor(np.asarray([s[1] for s in sample]))

        # if terminated: reward,
        # else: reward + discount * max(target)
        y = torch.stack([tensor_rewards[idx] if is_terminating(state) else tensor_rewards[idx] +
                         discount_factor_tensor * torch.max(target_values[idx]) for idx, state in enumerate(current_states)]).to(torch.float32).view(-1, 1)

        loss = loss_in_epoch(model, criterion, sample, y)
        loss_in_session.append(loss.item())

        optimizer.zero_grad()
        loss.backward()
        optimizer.step()
        print(f"\tAfter learning loss: {loss_in_epoch(model, criterion, sample, y)}")
    print('end of training session')
    return loss_in_session
