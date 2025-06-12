using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ServerHeartbeat : MonoBehaviour
{
    private TcpListener listener;
    private bool running;

    public static int port = 7778;

    private async void Start()
    {
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        running = true;
        Debug.Log($"[Heartbeat] Listening on TCP port {port}");

        while (running)
        {
            try
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                _ = HandleClient(client); // fire-and-forget
            }
            catch (SocketException ex)
            {
                Debug.LogWarning($"[Heartbeat] SocketException: {ex.Message}");
            }
        }
    }

    private async Task HandleClient(TcpClient client)
    {
        using (client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] message = Encoding.ASCII.GetBytes("ALIVE");
                await stream.WriteAsync(message, 0, message.Length);
                await stream.FlushAsync();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[Heartbeat] Client handling failed: {ex.Message}");
            }
        }
    }

    private void OnApplicationQuit()
    {
        running = false;
        listener?.Stop();
        Debug.Log("[Heartbeat] Listener stopped.");
    }
}