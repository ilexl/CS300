using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Automatically starts a Netcode client when the NetworkManager becomes available.
/// Useful for quick connection testing or automated joining in non-dedicated builds.
/// </summary>
public class PlayerJoin : MonoBehaviour
{
    /// <summary>
    /// Called once when the script instance is first loaded.
    /// Begins checking for NetworkManager initialization.
    /// </summary>
    void Start()
    {
        StartCoroutine(WaitForNetworkManager());
    }

    /// <summary>
    /// Coroutine that waits until the NetworkManager exists in the scene,
    /// then starts the client connection.
    /// </summary>
    System.Collections.IEnumerator WaitForNetworkManager()
    {
        while (NetworkManager.Singleton == null)
        {
            Debug.Log("Waiting for NetworkManager to be initialized...");
            yield return null; // Wait one frame
        }

        Debug.Log("NetworkManager found, starting client.");
        NetworkManager.Singleton.StartClient();
    }
}
