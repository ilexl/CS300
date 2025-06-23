using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

/// <summary>
/// Resolves a URL to an IP address at runtime and configures UnityTransport.
/// Also provides an async method to check server heartbeat status.
/// </summary>
public class URLToIP : MonoBehaviour
{
    public string URL;
    [SerializeField] UnityTransport transport;

    /// <summary>
    /// Resolves the provided URL to an IP address and sets connection data on UnityTransport.
    /// </summary>
    private void Start()
    {
        if (URL == "") { return; }
        if (transport == null) { return; }

        var ip = URLtoIP(URL);

        Debug.Log($"{URL} => {ip}");
        transport.SetConnectionData(ip.ToString(), 7777); 
    }

    /// <summary>
    /// Resolves a hostname or URL to its first available IPv4 address.
    /// </summary>
    /// <param name="url">The hostname or URL to resolve.</param>
    /// <returns>The resolved IPAddress.</returns>
    public static IPAddress URLtoIP(string url)
    {
        IPAddress ip = System.Net.Dns.GetHostEntry(url).AddressList[0];
        return ip;
    }

    /// <summary>
    /// Attempts to connect to a TCP server at the specified IP and port,
    /// and checks for an "ALIVE" response string to confirm server status.
    /// </summary>
    /// <param name="ipAddress">Target server IP address.</param>
    /// <param name="port">Port to connect to (default is 9000).</param>
    /// <param name="timeout">Timeout in milliseconds (default is 2000).</param>
    /// <returns>True if the server responds with "ALIVE"; otherwise false.</returns>
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
