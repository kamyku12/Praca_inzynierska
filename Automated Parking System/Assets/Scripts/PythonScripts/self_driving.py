import socket
import time
import random
from actionMap import getActionList

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



def handle_received_data(received_data, calculations):
    newPaused = False
    newStopped = False
    newEpisode = False
    newCalculations = calculations
    
    if received_data == 'stop':
        print('stop')
        newStopped = True
        newCalculations = True

    if received_data == 'pause':
        print('pause')
        newPaused = True
        newCalculations = True

    if received_data == 'newEpisode':
        print('newEpisode')
        newPaused = True
        newEpisode = True

    if received_data == 'unPause':
        print('unPause')
        newPaused = False
        newCalculations = True

    return newPaused, newStopped, newEpisode, newCalculations

def main():
    paused = True
    stopped = False
    initializing = True
    newEpisode = False
    calculations = True

    host, port = "127.0.0.1", 25001
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.settimeout(3)
    actions = getActionList()
    acceleration, rotation, braking = random.choice(actions)

    print("Now start Unity simulator!")
    connect(sock, host, port)

    print("Started listening")
    while not stopped:
        if initializing:
            initializing = initialize(sock)
        else:
            if not paused:
                print('sending')
                if random.random() < 0.01:
                    acceleration, rotation, braking = random.choice(actions)
                move_car(sock, acceleration, rotation, braking)

            if paused and not newEpisode: 
                sock.sendall("waitingForUnpause".encode("UTF-8"))
                print('Waiting to unpause')

            if newEpisode:
                if calculations:
                    sock.sendall("newEpisodeCalculations".encode("UTF-8"))
                    print("New episode calculations")
                    time.sleep(3)
                    calculations = False
                else:
                    sock.sendall("waitingForUnpause".encode("UTF-8"))

            received_data = sock.recv(1024).decode("UTF-8")
            paused, stopped, newEpisode, calculations = handle_received_data(received_data, calculations)
    

    input('-press any key to end-')

if __name__ == "__main__":
    main()
    