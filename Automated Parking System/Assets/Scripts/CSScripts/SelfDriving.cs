using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Globalization;
using System.Diagnostics;

public class SelfDriving : MonoBehaviour
{
    float receivedMotor;
    float receivedSteering;
    float receivedBreaking;

    Thread mThread;
    public string connectionIp = "127.0.0.1";
    public int connectionPort = 25001;
    IPAddress localAdd;
    TcpListener listener;
    TcpClient client;
    bool running, started, stop;
    Process proc;

    private void Start()
    {
        proc = new Process();
        string path = Application.dataPath + "/Scripts/PythonScripts/self_driving.py";
        UnityEngine.Debug.Log(path);
        proc.StartInfo.FileName = path;
        proc.Start();
        started = false;
        ThreadStart ts = new(GetInfo);
        mThread = new Thread(ts);
        mThread.Start();
    }

    private void OnApplicationQuit()
    {
        if (mThread != null)
            mThread.Abort();
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



        if (!started)
        {
            byte[] data = Encoding.ASCII.GetBytes("start");
            nwStream.Write(data, 0, data.Length);
        }
        else
        {
            if (stop)
            {
                byte[] data = Encoding.ASCII.GetBytes("stop");
                nwStream.Write(data, 0, data.Length);
            }
        }

        if (started && dataReceived != null)
        {
            float[] parsedData = ParseData(dataReceived);
            print(parsedData);
            receivedMotor = parsedData[0];
            receivedSteering = parsedData[1];
            receivedBreaking = parsedData[2];
            print("received motor and steering, moving Car...");

            byte[] myWriteBuffer = Encoding.ASCII.GetBytes("ok");
            nwStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
        }

        if (dataReceived.Equals("started"))
            started = true;
        else if (dataReceived.Equals("stop"))
            running = false;
    }

    private float[] ParseData(string data)
    {
        string[] sArray = data.Split("|");
        float motor = float.Parse(sArray[0], CultureInfo.InvariantCulture.NumberFormat);
        float steering = float.Parse(sArray[1], CultureInfo.InvariantCulture.NumberFormat);
        float breaking = float.Parse(sArray[2], CultureInfo.InvariantCulture.NumberFormat);
        return new float[] { motor, steering, breaking };
    }


    private void HandleDataReceived()
    {
        //To Do

    }

    /*
    static Vector3 StringToVector3(string sVector)
    {
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        string[] sArray = sVector.Split(",");
        return new Vector3(float.Parse(sArray[0]),
                           float.Parse(sArray[1]),
                           float.Parse(sArray[2]));
    }
    */

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
        return receivedBreaking;
    }

    public void StartStop()
    {
        stop = !stop;
    }
}
