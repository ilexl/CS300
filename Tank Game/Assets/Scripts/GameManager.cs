using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Represents the different gameplay modes supported by the game.
/// Used by GameManager to control game rules and flow based on mode.
/// </summary>
public enum GameMode 
{ 
    FreeForAll, 
    TeamDeathmatch, 
    CaptureTheFlag 
}


/// <summary>
/// Manages the current game mode and synchronizes it across the network.
/// Implements server-authoritative game mode setting with client updates via RPC.
/// Uses a singleton pattern for easy global access.
/// </summary>
public class GameManager : NetworkBehaviour
{
    public static GameManager Singleton { get; private set; }

    [SerializeField] GameMode currentGameMode;

    /// <summary>
    /// Returns the currently active game mode.
    /// </summary>
    public GameMode GetCurrentGamemode()
    {
        return currentGameMode;
    }

    /// <summary>
    /// Coroutine that waits until NetworkManager is initialized,
    /// then spawns the GameManager network object on the server
    /// and applies the initial game mode setting.
    /// Also assigns the singleton instance.
    /// </summary>
    private void Start()
    {
        StartCoroutine(WaitForNetwork());
    }

    private System.Collections.IEnumerator WaitForNetwork()
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
            SetGameMode(currentGameMode); // Initialize game mode on all clients
        }
        Singleton = this;
    }

    /// <summary>
    /// Server-only method to set the current game mode.
    /// Updates the server state and broadcasts the change to clients.
    /// </summary>
    /// <param name="newMode">The new game mode to set.</param>
    public void SetGameMode(GameMode newMode)
    {
        if (!IsServer)
        {
            Debug.LogWarning("Only the server can set the game mode.");
            return;
        }

        currentGameMode = newMode;

        // Notify all clients of the updated game mode
        BroadcastGameModeClientRpc(currentGameMode);
        Debug.Log($"Server set game mode to {newMode}");
    }

    /// <summary>
    /// Client RPC called by the server to update the game mode on all clients.
    /// Clients update their local game mode state accordingly.
    /// </summary>
    /// <param name="mode">The game mode sent by the server.</param>
    [ClientRpc]
    private void BroadcastGameModeClientRpc(GameMode mode)
    {
        if (IsServer) return; // Avoid updating on server again

        currentGameMode = mode;
        Debug.Log($"Client updated game mode to {mode}");
    }
}
