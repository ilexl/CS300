using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Networking;

public class URLToIP : MonoBehaviour
{
    public string URL;
    [SerializeField] UnityTransport transport;

    private void Start()
    {
        if (URL == "") { return; }
        if (transport == null) { return; }

        var ip = URLtoIP(URL);

        Debug.Log($"{URL} => {ip}");
        transport.SetConnectionData(ip.ToString(), 7777); 
    }

    public static IPAddress URLtoIP(string url)
    {
        IPAddress ip = System.Net.Dns.GetHostEntry(url).AddressList[0];
        return ip;
    }

    public static async Task<bool> IsUnityServerAlive(string ipAddress, int port = 9000, int timeout = 2000)
    {
        try
        {
            using (TcpClient tcpClient = new TcpClient())
            {
                var connectTask = tcpClient.ConnectAsync(ipAddress, port);
                var timeoutTask = Task.Delay(timeout);
                var completed = await Task.WhenAny(connectTask, timeoutTask);

                if (completed != connectTask)
                    return false; // Timed out

                await connectTask; // Ensure exception is thrown if failed

                using (var stream = tcpClient.GetStream())
                {
                    byte[] buffer = new byte[16];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    return response == "ALIVE";
                }
            }
        }
        catch
        {
            return false; // Could not connect or read
        }
    }

}
