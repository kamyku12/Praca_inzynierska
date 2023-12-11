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
    public string receivedData;
    public SendingDataEvents sendDataEvent;
    public int connectionPort = 25001;
    IPAddress localAdd;
    TcpListener listener;
    TcpClient client;
    NetworkStream nwStream;
    public bool running, pause;
    Process proc;
    public LearningArtificialBrain learningArtificialBrain;
    public ObservationForRL ObservationForRL;

    private void Start()
    {
        proc = new Process();
        string path = Application.dataPath + "/Scripts/PythonScripts/self_driving.py";
        UnityEngine.Debug.Log(path);
        proc.StartInfo.FileName = path;
        proc.Start();
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

        if(listener != null)
        {
            listener.Stop();
        }
        sendDataEvent = SendingDataEvents.Stop;
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
        nwStream = client.GetStream();
        byte[] buffer = new byte[client.ReceiveBufferSize];

        int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);
        receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        HandleReceivingData();

        HandleSendingData();
    }

    private float[] ParseData(string data)
    {
        string[] sArray = data.Split("|");
        float motor = float.Parse(sArray[0], CultureInfo.InvariantCulture.NumberFormat);
        float steering = float.Parse(sArray[1], CultureInfo.InvariantCulture.NumberFormat);
        float braking = float.Parse(sArray[2], CultureInfo.InvariantCulture.NumberFormat);
        return new float[] { motor, steering, braking };
    }


    private void HandleSendingData()
    {
        switch(sendDataEvent)
        {
            // If event is start, send to python script message to start working
            case SendingDataEvents.Start:
                Send("start");
                break;
            // If event is stop, send to python script message to stop working
            case SendingDataEvents.Stop:
                Send("stop");
                break;
            // If event is new episode, send to python script message about creating new episode
            case SendingDataEvents.NewEpisode:
                Send("newEpisode");
                // Force brake on new episode to stop car from moving
                receivedBraking = 1;
                break;
            // If event is received data, send to python script message that car received data
            case SendingDataEvents.ReceivedData:
                Send(ObservationForRL.GetObservations());
                break;
            // If event is pause, send to python script message to pause sending data
            case SendingDataEvents.Pause:
                Send("pause");
                break;
            case SendingDataEvents.UnPause:
                Send("unPause");
                break;
            case SendingDataEvents.Idle:
                Send("waiting");
                break;
            default:
                break;
        }
    }

    private void Send(string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        nwStream.Write(data, 0, data.Length);
    }

    private void HandleReceivingData()
    {
        if (sendDataEvent == SendingDataEvents.NewEpisode && receivedData != "newEpisodeCalculations") 
        {
            return;
        }

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
                learningArtificialBrain.SetCanRun(true);
                float[] parsedData = ParseData(receivedData);
                receivedMotor = parsedData[0];
                receivedSteering = parsedData[1];
                receivedBraking = parsedData[2];

                if(!pause)
                {
                    sendDataEvent = SendingDataEvents.ReceivedData;
                }
                break;
            case "newEpisodeCalculations":
                sendDataEvent = SendingDataEvents.UnPause;
                return;
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
            print("Pause");
            sendDataEvent = SendingDataEvents.Pause;
        }
        else
        {
            print("unPause");
            sendDataEvent = SendingDataEvents.UnPause;
        }
    }

    public void SetSendingDataEvent(SendingDataEvents newEvent)
    {
        sendDataEvent = newEvent;
    }

    public void ResetValues()
    {
        receivedMotor = 0;
        receivedSteering = 0;
        receivedBraking = 0;
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
