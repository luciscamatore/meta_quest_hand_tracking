using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;

public class HandDataSender : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;

    void Start()
    {
        try{
            client = new TcpClient("127.0.0.1", 5005);
            stream = client.GetStream();
        }catch (Exception e){
            Debug.LogError("Error connecting to server: " + e.Message);
        }
    }

    public void SendHandData(string jsonData)
    {
        if (stream != null && stream.CanWrite)
        {
            byte[] data = Encoding.UTF8.GetBytes(jsonData + "\n");
            stream.Write(data, 0, data.Length);
        }
    }

    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
    }

}
