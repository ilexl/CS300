using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages player connections and disconnections in a multiplayer game.
/// Tracks connected players via NetworkObjects and handles their cleanup on disconnect.
/// Registers for connection callbacks once the NetworkManager is ready.
/// </summary>
public class MultiplayerManager : MonoBehaviour
{
    [SerializeField] private List<NetworkObject> playerList = new();

    /// <summary>
    /// Starts waiting for NetworkManager initialization before registering connection callbacks.
    /// </summary>
    private void Start()
    {
        StartCoroutine(WaitForNetworkManager());
    }

    /// <summary>
    /// Coroutine that waits until NetworkManager singleton is available,
    /// then initializes player list and hooks up connection/disconnection events.
    /// </summary>
    System.Collections.IEnumerator WaitForNetworkManager()
    {
        while (NetworkManager.Singleton == null)
        {
            Debug.Log("Waiting for NetworkManager to be initialized...");
            yield return null;
        }

        Debug.Log("NetworkManager found, MultiplayerManager Initialized!");
        playerList = new List<NetworkObject>();

        // Subscribe to connection and disconnection events on the server
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
    }

    /// <summary>
    /// Called when a client disconnects.
    /// Finds the corresponding player object, destroys it, and removes from the player list.
    /// </summary>
    /// <param name="clientId">The client ID of the disconnected player</param>
    private void HandleClientDisconnected(ulong clientId)
    {
        Debug.Log($"Player {clientId} disconnected");
        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i].OwnerClientId == clientId)
            {
                Destroy(playerList[i].gameObject);
                playerList.RemoveAt(i);
                break; // Exit loop once the player is found and removed
            }
        }
    }

    /// <summary>
    /// Called when a client connects.
    /// Currently logs connection; player spawning and initialization handled elsewhere.
    /// </summary>
    /// <param name="clientId">The client ID of the connected player</param>
    private void HandleClientConnected(ulong clientId)
    {
        Debug.Log($"Player {clientId} connected");
    }

    /// <summary>
    /// Removes a player by client ID from the player list and destroys their game object.
    /// Can be called forcibly (e.g., for kicking players).
    /// </summary>
    /// <param name="clientId">The client ID of the player to remove</param>
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

    /// <summary>
    /// Unsubscribes from network callbacks when this object is destroyed to avoid memory leaks.
    /// </summary>
    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) { return; }
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }
}
