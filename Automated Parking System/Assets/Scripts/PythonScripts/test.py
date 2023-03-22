import socket
import time
import numpy as np


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


def move_cube(sock, startPos):
    time.sleep(0.025)
    startPos[np.random.randint(0, 3)] += np.random.randint(-1, 2)
    posString = ','.join(map(str, startPos))
    print(posString)

    sock.sendall(posString.encode("UTF-8"))
    try:
        received_data = sock.recv(1024).decode("UTF-8")
        if received_data == "stop":
            return False
    except:
        return False
    print(received_data)
    return True


if __name__ == "__main__":
    paused = True
    stopped = False
    initializing = True
    host, port = "127.0.0.1", 25001
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.settimeout(3)

    print("Now start Unity simulator!")
    connect(sock, host, port)

    startPos = [0, 0, 0]

    print("Started listening")
    while True:
        if initializing:
            paused, initializing = initialize()
        else:
            if not paused:
                if not move_cube(sock, startPos):
                    break

    sock.sendall("stop".encode("UTF-8"))
