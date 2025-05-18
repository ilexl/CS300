using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine;

public enum GameMode { FreeForAll, TeamDeathmatch, CaptureTheFlag }

public class GameManager : NetworkBehaviour
{
    public static GameManager Singleton { get; private set; }

    [SerializeField] GameMode currentGameMode;

    public GameMode GetCurrentGamemode()
    {
        return currentGameMode;
    }

    void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        StartCoroutine(Spawn());
    }

    private System.Collections.IEnumerator Spawn()
    {
        while (NetworkManager.Singleton == null)
        {
            Debug.Log("Waiting for NetworkManager...");
            yield return null;
        }

        if (IsServer)
        {
            GetComponent<NetworkObject>().Spawn(true);
            Debug.Log("Game Manager spawned on network.");
            SetGameMode(currentGameMode); // set gamemode for all clients as the servers selected gamemode
        }
    }

    // Server-side function to set the game mode and inform all clients
    public void SetGameMode(GameMode newMode)
    {
        if (!IsServer)
        {
            Debug.LogWarning("Only the server can set the game mode.");
            return;
        }

        currentGameMode = newMode;

        // Inform all clients about the new game mode
        BroadcastGameModeClientRpc(currentGameMode);
        Debug.Log($"Server set game mode to {newMode}");
    }

    // RPC to update game mode on all clients
    [ClientRpc]
    private void BroadcastGameModeClientRpc(GameMode mode)
    {
        if (IsServer) return;

        currentGameMode = mode;
        Debug.Log($"Client updated game mode to {mode}");
    }
}
