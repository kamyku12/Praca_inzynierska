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


def move_car(sock, last_steering, up_down):
    time.sleep(0.025)
    motor = 0.5
    steering = last_steering + 0.05 * up_down

    if steering >= 1:
        up_down = -1
    elif steering <= -1:
        up_down = 1

    data_to_send = "|".join(map(str, [motor, steering]))
    print(data_to_send)
    sock.sendall(data_to_send.encode("UTF-8"))

    received_data = sock.recv(1024).decode("UTF-8")
    if received_data == "stop":
        return None

    return steering, up_down


if __name__ == "__main__":
    paused = True
    stopped = False
    initializing = True
    host, port = "127.0.0.1", 25001
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.settimeout(3)
    steering = 0
    up_down = 1
    stop = 1

    print("Now start Unity simulator!")
    connect(sock, host, port)

    startPos = [0, 0, 0]

    print("Started listening")
    while True:
        if initializing:
            paused, initializing = initialize()
        else:
            if not paused:
                steering, up_down = move_car(sock, steering, up_down)
                if steering is None:
                    break

    sock.sendall("stop".encode("UTF-8"))
