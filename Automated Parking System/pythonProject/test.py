import socket
import time
import numpy as np

paused = True
stopped = False
connecting = True
host, port = "127.0.0.1", 25001
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.settimeout(3)

print("Now start Unity simulator!")
error = True
while error:
    try:
        sock.connect((host, port))
        error = False
    except Exception as msg:
        print(msg)
        error = True

startPos = [0, 0, 0]

print("Started listening")
while True:
    receivedData = None
    if connecting:
        try:
            sock.sendall("waiting for start".encode("UTF-8"))
            receivedData = sock.recv(1024).decode("UTF-8")
        except:
            pass

        if receivedData == "start":
            print("started")
            paused = False
            connecting = False
            sock.sendall("started".encode("UTF-8"))
    else:
        if not paused:
            time.sleep(0.025)
            startPos[np.random.randint(0, 3)] += np.random.randint(-1, 2)
            posString = ','.join(map(str, startPos))
            print(posString)

            sock.sendall(posString.encode("UTF-8"))
            try:
                receivedData = sock.recv(1024).decode("UTF-8")
            except:
                break
            print(receivedData)
