using System;
using System.Net;
using System.Net.Sockets;
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

    public static async Task<bool> IsPortOpenAsync(string ipAddress, int port, int timeout = 2000)
    {
        using (TcpClient tcpClient = new TcpClient())
        {
            try
            {
                var connectTask = tcpClient.ConnectAsync(ipAddress, port);
                if (await Task.WhenAny(connectTask, Task.Delay(timeout)) == connectTask)
                {
                    return tcpClient.Connected;
                }
                else
                {
                    return false; // Timeout
                }
            }
            catch (SocketException)
            {
                return false; // Connection failed
            }
            catch (Exception)
            {
                return false; // Other errors
            }
        }
    }
}
