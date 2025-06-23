using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Responsible for starting the server using Unity Netcode's NetworkManager.
/// Waits for the NetworkManager to initialize before starting the server.
/// </summary>
public class StartServer : MonoBehaviour
{
    /// <summary>
    /// Called once on component creation; begins server startup coroutine if no server exists.
    /// </summary>
    void Start()
    {
        if(NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            Debug.LogError("Network Manager already exists - stopping execution...");
            gameObject.SetActive(false);
            return;
        }
        Debug.Log("Server");
        StartCoroutine(WaitForNetworkManager());
    }

    /// <summary>
    /// Coroutine that waits for the NetworkManager singleton to be ready, then starts the server.
    /// </summary>
    System.Collections.IEnumerator WaitForNetworkManager()
    {
        while (NetworkManager.Singleton == null)
        {
            Debug.Log("Waiting for NetworkManager to be initialized...");
            yield return null; // Wait one frame
        }

        Debug.Log("NetworkManager found, starting server!");
        NetworkManager.Singleton.StartServer();
    }
}
