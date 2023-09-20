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



def handle_received_data(received_data):
    newPaused = False
    newStopped = False
    newEpisode = False
    
    if received_data == 'stop':
        newStopped = True

    if received_data == 'pause':
        newPaused = True

    if received_data == 'newEpisode':
        newPaused = True
        newEpisode = True

    if received_data == 'unPause':
        print('unPause')
        newPaused = False

    return newPaused, newStopped, newEpisode

def main():
    paused = True
    stopped = False
    initializing = True
    newEpisode = False

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
                if random.random() < 0.01:
                    acceleration, rotation, braking = random.choice(actions)
                move_car(sock, acceleration, rotation, braking)

            if paused: 
                sock.sendall("waitingForUnpause".encode("UTF-8"))
                print('Waiting to unpause')

            received_data = sock.recv(1024).decode("UTF-8")
            paused, stopped, newEpisode = handle_received_data(received_data)
    

    input('-press any key to end-')

if __name__ == "__main__":
    main()
    