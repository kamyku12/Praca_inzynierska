import socket
import time
import random
from actionMap import getActionList
import torch
from ReplayMemory import ReplayMemory
import numpy as np
from matplotlib import pyplot
from utils import handle_received_data, create_tensor_from_state, create_nn_models, training
import os


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


def move_car(sock, acceleration, rotation):
    time.sleep(0.02)

    data_to_send = "|".join(map(str, [acceleration, rotation, 0]))
    sock.sendall(data_to_send.encode("UTF-8"))


def main():
    paused = True
    stopped = False
    initializing = True
    newEpisode = False
    calculations = True
    episodes_counter = 0
    rewards_memory = []
    loss_memory = []
    rewards_for_plot = []
    cur_state_tensor = None

    reward = 0

    curr_state = dict(
        velocity_x=0,
        velocity_z=0,
        rotation=0,
        isCarInsideSpot=False,
        f_distance=0,
        b_distance=0,
        timer=0.0
    )
    next_state = curr_state.copy()

    host, port = "127.0.0.1", 25001
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.settimeout(3)
    actions = getActionList()
    acceleration, rotation = random.choice(actions)
    picked_action = None

    # hyperparameters

    # random action epsilon - % of chance to pick random action
    # instead of action chosen by value approximation
    # it is used to introduce exploration into our reinforcement learning
    random_action = 0.8
    random_action_decay = 0.9
    min_random_action = 0.1

    # learning rate of neural network
    learning_rate = 0.001

    # size of replay memory used in learning
    replay_memory_size = 100_000

    # after which number of episodes update weights of second neural network
    episodes_to_update = 10

    # discount factor
    discount_factor = 0.9

    # number of episodes to train model
    episodes_to_train = 1

    # bach size for training
    batch_size = 100

    # ---------------

    experienceMemory = ReplayMemory(
        replay_memory_size, batch_size)

    model, target_model, criterion, optimizer = create_nn_models(
        curr_state, actions, learning_rate)

    print("Now start Unity simulator!")
    connect(sock, host, port)

    print("Started listening")
    while not stopped:
        if initializing:
            initializing = initialize(sock)
        else:
            if not paused:
                if random.random() < random_action:
                    picked_action = random.randint(0, len(actions) - 1)
                    acceleration, rotation = actions[picked_action]
                else:
                # create tensor from state
                    cur_state_tensor = create_tensor_from_state(curr_state)

                    # Get value of model
                    value = model(cur_state_tensor)

                    # Pick action
                    picked_action = torch.argmax(value)

                    acceleration, rotation = actions[picked_action]

                move_car(sock, acceleration, rotation)

                rewards_memory.append(reward)

            if paused and not newEpisode:
                sock.sendall("waitingForUnpause".encode("UTF-8"))
                # print('Waiting to unpause')

            if newEpisode:
                if calculations:
                    episodes_counter += 1
                    sock.sendall("newEpisodeCalculations".encode("UTF-8"))

                    # train model after episodes_to_train number of episodes
                    if episodes_counter % episodes_to_train == 0:
                        loss_in_training = training(experienceMemory, criterion,
                                 optimizer, model, target_model, discount_factor)
                        loss_memory.extend(loss_in_training)
                        

                    # update target_model after episodes_to_update number of episodes
                    if episodes_counter % episodes_to_update == 0:
                        print('update')
                        target_model.load_state_dict(model.state_dict())

                    # Get average of rewards for plot
                    rewards_for_plot.append(np.average(rewards_memory))
                    rewards_memory = []

                    # update random action probability
                    random_action *= random_action_decay
                    if random_action < min_random_action:
                        random_action = min_random_action

                    calculations = False
                else:
                    sock.sendall("waitingForUnpause".encode("UTF-8"))

            try:
                received_data = sock.recv(1024).decode("UTF-8")
                curr_state = next_state.copy()
                paused, stopped, newEpisode, calculations, next_state, reward = handle_received_data(
                    received_data, calculations, next_state, reward)

                if picked_action:
                    experience = [curr_state,
                                  reward, next_state]
                    experienceMemory.push(experience)

            except Exception as e:
                print(e)
                stopped = True

    # save model to SelfDrivingModel.pth file to update this model
    # in another iteration of project
    print('saving...')
    torch.save(model.state_dict(
    ), f"{os.getcwd()}/Assets/Scripts/PythonScripts/SelfDrivingModel.pth")

    # plot reward changes
    pyplot.subplot(211)
    pyplot.title("Rewards")
    pyplot.plot(rewards_for_plot)
    pyplot.subplot(212)
    pyplot.title("Loss")
    pyplot.plot(np.arange(1, len(loss_memory) + 1) / 10, loss_memory)
    pyplot.show()


if __name__ == "__main__":
    try:
        # your code, typically one function call
        main()
    except:
        import sys
        print(sys.exc_info()[0])
        import traceback
        print(traceback.format_exc())
    finally:
        print("Press Enter to continue ...")
        input()
