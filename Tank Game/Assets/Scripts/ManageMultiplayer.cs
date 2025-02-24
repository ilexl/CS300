using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ManageMultiplayer : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;
    [SerializeField] List<NetworkObject> playerList;
    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            playerList = new List<NetworkObject>();
            // This method is called when a new player joins the server.
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        }
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        Debug.Log($"Player {clientId} disconnected");
        foreach(NetworkObject obj in playerList)
        {
            if(obj.OwnerClientId == clientId)
            {
                Destroy(obj);
                playerList.Remove(obj);
                return;
            }
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        Debug.Log($"Player {clientId} connected");
        NetworkObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<NetworkObject>();
        player.SpawnAsPlayerObject(clientId);
        playerList.Add(player);
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) { return; }
        if (NetworkManager.Singleton.IsServer)
        {
            // Remove the callback when the object is destroyed
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        }
    }
}
