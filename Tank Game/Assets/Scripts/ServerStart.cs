using UnityEngine;
using Unity.Netcode;

public class StartServer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Server");
        StartCoroutine(WaitForNetworkManager());
    }

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
