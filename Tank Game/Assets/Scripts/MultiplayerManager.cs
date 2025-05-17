using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerManager : MonoBehaviour
{
    [SerializeField] private List<NetworkObject> playerList = new();
    private void Start()
    {
        StartCoroutine(WaitForNetworkManager());
    }

    System.Collections.IEnumerator WaitForNetworkManager()
    {
        while (NetworkManager.Singleton == null)
        {
            Debug.Log("Waiting for NetworkManager to be initialized...");
            yield return null; // Wait one frame
        }

        Debug.Log("NetworkManager found, MultiplayerManager Initialized!");
        playerList = new List<NetworkObject>();
        // This method is called when a new player joins the server.
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        Debug.Log($"Player {clientId} disconnected");
        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i].OwnerClientId == clientId)
            {
                Destroy(playerList[i].gameObject);
                playerList.RemoveAt(i);
                break;
            }
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        Debug.Log($"Player {clientId} connected");

        // Player spawning is handled separately (e.g., after team selection)
    }

    public void RemovePlayer(ulong clientId)
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i].OwnerClientId == clientId)
            {
                Destroy(playerList[i].gameObject);
                playerList.RemoveAt(i);
                Debug.Log($"Player {clientId} forcibly removed");
                break;
            }
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) { return; }
        if (NetworkManager.Singleton.IsServer)
        {
            // Remove the callback when the object is destroyed
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }
}
