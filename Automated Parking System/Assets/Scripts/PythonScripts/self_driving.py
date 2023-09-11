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


def initialize():
    received_data_initialize = None
    try:
        sock.sendall("waiting for start".encode("UTF-8"))
        received_data_initialize = sock.recv(1024).decode("UTF-8")
    except:
        pass

    if received_data_initialize == "start":
        print("started")
        sock.sendall("started".encode("UTF-8"))
        return False, False

    return True, True


def move_car(sock, acceleration, rotation, braking):
    time.sleep(0.025)

    data_to_send = "|".join(map(str, [acceleration, rotation, braking]))
    print(data_to_send)
    sock.sendall(data_to_send.encode("UTF-8"))

    received_data = sock.recv(1024).decode("UTF-8")
    if received_data == "stop":
        return True

    return False


if __name__ == "__main__":
    paused = True
    stopped = False
    initializing = True
    host, port = "127.0.0.1", 25001
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.settimeout(3)
    actions = getActionList()

    print("Now start Unity simulator!")
    connect(sock, host, port)

    startPos = [0, 0, 0]

    print("Started listening")
    while stopped:
        if initializing:
            paused, initializing = initialize()
        else:
            if not paused:
                acceleration, rotation, braking = random.choice(actions)
                stopped = move_car(sock, acceleration, rotation, braking)


    sock.sendall("stop".encode("UTF-8"))