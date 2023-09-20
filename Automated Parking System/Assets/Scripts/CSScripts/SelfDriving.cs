using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Globalization;
using System.Diagnostics;
using System;

public class SelfDriving : MonoBehaviour
{
    float receivedMotor;
    float receivedSteering;
    float receivedBraking;

    Thread mThread;
    public string connectionIp = "127.0.0.1";
    public SendingDataEvents sendDataEvent;
    public int connectionPort = 25001;
    IPAddress localAdd;
    TcpListener listener;
    TcpClient client;
    public bool running, pause;
    Process proc;

    private void Start()
    {
        //proc = new Process();
        //string path = Application.dataPath + "/Scripts/PythonScripts/self_driving.py";
        //UnityEngine.Debug.Log(path);
        //proc.StartInfo.FileName = path;
        //proc.Start();
        sendDataEvent = SendingDataEvents.Start;
        ThreadStart ts = new(GetInfo);
        mThread = new Thread(ts);
        mThread.Start();
        pause = true;
    }

    private void OnApplicationQuit()
    {
        if (mThread != null)
        {
            print("Aborting thread");
            mThread.Abort();
        }
        
        if(proc != null)
        {
            print("Killing process");
            proc.Kill();
        }
    }


    void GetInfo()
    {
        localAdd = IPAddress.Parse(connectionIp);
        listener = new TcpListener(IPAddress.Any, connectionPort);
        listener.Start();

        client = listener.AcceptTcpClient();

        running = true;
        while (running)
        {
            SendAndReceiveData();
        }
        listener.Stop();
    }

    void SendAndReceiveData()
    {
        NetworkStream nwStream = client.GetStream();
        byte[] buffer = new byte[client.ReceiveBufferSize];

        int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);
        string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        HandleReceivingData(dataReceived);

        HandleSendingData(nwStream);
    }

    private float[] ParseData(string data)
    {
        string[] sArray = data.Split("|");
        float motor = float.Parse(sArray[0], CultureInfo.InvariantCulture.NumberFormat);
        float steering = float.Parse(sArray[1], CultureInfo.InvariantCulture.NumberFormat);
        float braking = float.Parse(sArray[2], CultureInfo.InvariantCulture.NumberFormat);
        return new float[] { motor, steering, braking };
    }


    private void HandleSendingData(NetworkStream nwStream)
    {
        byte[] data;
        switch(sendDataEvent)
        {
            // If event is start, send to python script message to start working
            case SendingDataEvents.Start:
                data = Encoding.ASCII.GetBytes("start");
                nwStream.Write(data, 0, data.Length);
                break;
            // If event is stop, send to python script message to stop working
            case SendingDataEvents.Stop:
                data = Encoding.ASCII.GetBytes("stop");
                nwStream.Write(data, 0, data.Length);
                break;
            // If event is new episode, send to python script message about creating new episode
            case SendingDataEvents.NewEpisode:
                data = Encoding.ASCII.GetBytes("newEpisode");
                nwStream.Write(data, 0, data.Length);
                break;
            // If event is received data, send to python script message that car received data
            case SendingDataEvents.ReceivedData:
                data = Encoding.ASCII.GetBytes("receivedData");
                nwStream.Write(data, 0, data.Length);
                break;
            // If event is pause, send to python script message to pause sending data
            case SendingDataEvents.Pause:
                data = Encoding.ASCII.GetBytes("pause");
                nwStream.Write(data, 0, data.Length);
                break;
            case SendingDataEvents.UnPause:
                data = Encoding.ASCII.GetBytes("unPause");
                nwStream.Write(data, 0, data.Length);
                break;
            default:
                break;
        }
    }

    private void HandleReceivingData(string receivedData)
    {
        print(receivedData);
        switch(receivedData)
        {
            case "started":
                sendDataEvent = SendingDataEvents.Running;
                break;
            case "stop":
                running = false;
                break;
            case "waitingForStart":
                sendDataEvent = SendingDataEvents.Start;
                break;
            case "waitingForUnpause":
                if(!pause)
                {
                    sendDataEvent = SendingDataEvents.UnPause;
                }
                else
                {
                    sendDataEvent = SendingDataEvents.Pause;
                }
                break;
            case string s when s.Contains('|'):
                float[] parsedData = ParseData(receivedData);
                receivedMotor = parsedData[0];
                receivedSteering = parsedData[1];
                receivedBraking = parsedData[2];

                if(!pause)
                {
                    sendDataEvent = SendingDataEvents.ReceivedData;
                }
                break;
            default:
                sendDataEvent = SendingDataEvents.Idle;
                break;
        }
    }

    public float GetMotor()
    {
        return receivedMotor;
    }

    public float GetSteering()
    {
        return receivedSteering;
    }

    public float GetBrake()
    {
        return receivedBraking;
    }

    public void StartStop()
    {
        pause = !pause;
        if(pause)
        {
            print("Pase");
            sendDataEvent = SendingDataEvents.Pause;
        }
        else
        {
            print("unPause");
            sendDataEvent = SendingDataEvents.UnPause;
        }
    }
}

public enum SendingDataEvents
{
    Start,
    Stop,
    NewEpisode,
    Running,
    ReceivedData,
    Pause,
    UnPause,
    Idle
}
