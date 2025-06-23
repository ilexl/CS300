using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// TCP server that listens for client connections and responds with a heartbeat message.
/// Used to confirm the server is alive and responsive.
/// </summary>
public class ServerHeartbeat : MonoBehaviour
{

    public static int port = 7778; // Port number the server listens on.
    private TcpListener listener; // TCP listener that accepts incoming connections.
    private bool running; // Flag indicating whether the server should keep running.

    /// <summary>
    /// Starts the TCP listener and asynchronously handles incoming connections.
    /// </summary>
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
                _ = HandleClient(client); // Handle client asynchronously without awaiting
            }
            catch (SocketException ex)
            {
                Debug.LogWarning($"[Heartbeat] SocketException: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Handles communication with a connected client by sending a heartbeat message.
    /// </summary>
    /// <param name="client">Connected TCP client.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
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

    /// <summary>
    /// Stops the TCP listener and ends the server when the application quits.
    /// </summary>
    private void OnApplicationQuit()
    {
        running = false;
        listener?.Stop();
        Debug.Log("[Heartbeat] Listener stopped.");
    }
}