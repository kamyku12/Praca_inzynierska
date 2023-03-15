using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;
using System.Threading;
using UnityEngine;

public class SocketTest : MonoBehaviour
{
    Thread mThread;
    public string connectionIp = "127.0.0.1";
    public int connectionPort = 25001;
    IPAddress localAdd;
    TcpListener listener;
    TcpClient client;
    Vector3 receivedPos  = Vector3.zero;

    bool running, started = false;

    void Update()
    {
        if(started)
        {
            transform.position = receivedPos;
        }
    }

    private void Start()
    {
        started = false;
        ThreadStart ts = new ThreadStart(GetInfo);
        mThread = new Thread(ts);
        mThread.Start();
    }

    private void OnApplicationQuit()
    {
        mThread.Abort();
    }

    void GetInfo()
    {
        localAdd = IPAddress.Parse(connectionIp);
        listener = new TcpListener(IPAddress.Any, connectionPort);
        listener.Start();

        client = listener.AcceptTcpClient();

        running = true;
        while(running)
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
        print(dataReceived);


        if (!started)
        {
            byte[] data = Encoding.ASCII.GetBytes("start");
            nwStream.Write(data, 0, data.Length);
        }
        else if(dataReceived != null)
        {
            receivedPos = StringToVector3(dataReceived);
            print("received pos data, and moved the Cube");

            byte[] myWriteBuffer = Encoding.ASCII.GetBytes("hey i got your message");
            nwStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
        }
        
        if (dataReceived.Equals("started"))
            started = true;
    }

    static Vector3 StringToVector3(string sVector)
    {
        if(sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        string[] sArray = sVector.Split(",");
        return new Vector3(float.Parse(sArray[0]),
                           float.Parse(sArray[1]),
                           float.Parse(sArray[2]));
    }
}
