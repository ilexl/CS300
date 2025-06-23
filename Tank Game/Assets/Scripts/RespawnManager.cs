using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages player team assignments, spawn logic, tank selection,
/// and player death/respawn events in a multiplayer game.
/// </summary>
public class RespawnManager : NetworkBehaviour
{
    public static RespawnManager Singleton { get; private set; }

    [SerializeField] Transform TeamOrangeRespawn, TeamBlueRespawn;
    [SerializeField] float spawnRadius = 5f;

    private Dictionary<ulong, Team> playerTeams = new();

    /// <summary>
    /// Assigns the singleton reference on Awake.
    /// </summary>
    void Awake()
    {
        Singleton = this;
    }

    /// <summary>
    /// Ensures singleton reference is valid after Start.
    /// </summary>
    void Start()
    {
        Singleton = this;
    }

    /// <summary>
    /// Called by a client to select their team.
    /// Server assigns the team, spawns the player, and notifies all clients.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void SelectTeamServerRpc(Team team, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (playerTeams.ContainsKey(clientId))
        {
            RemovePlayerTeam(clientId);
        }

        playerTeams[clientId] = team;
        Debug.Log($"Client {clientId} joined {team} team");

        Vector3 spawnPos = GetRandomizedSpawnPosition(team);
        SendPlayerToSpawnClientRpc(clientId, spawnPos);

        NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerTeam>().SetTeamSide(team); // set the team
        NotifyTeamSelectedClientRpc(clientId, team);
    }

    /// <summary>
    /// Called on clients to apply team change locally after server assignment.
    /// </summary>
    [ClientRpc]
    private void NotifyTeamSelectedClientRpc(ulong clientId, Team team)
    {
        if (IsServer) { return; }
        Debug.Log($"Client {clientId} selected team {team}");

        NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerTeam>().SetTeamSide(team); // set the team
    }

    /// <summary>
    /// Returns a randomized position around the team’s spawn point.
    /// </summary>
    private Vector3 GetRandomizedSpawnPosition(Team team)
    {
        Transform baseSpawn = team switch
        {
            Team.Orange => TeamOrangeRespawn,
            Team.Blue => TeamBlueRespawn,
            _ => null
        };

        if (baseSpawn == null)
        {
            Debug.LogWarning($"Spawn point for team {team} is not set.");
            return Vector3.zero;
        }

        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 offset = new Vector3(randomCircle.x, 0, randomCircle.y);
        return baseSpawn.position + offset;
    }

    /// <summary>
    /// Moves the local client’s player to a new spawn position.
    /// Called by the server via ClientRpc.
    /// </summary>
    [ClientRpc]
    private void SendPlayerToSpawnClientRpc(ulong targetClientId, Vector3 spawnPosition)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        var player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (player != null)
        {
            player.transform.position = spawnPosition;
            player.transform.rotation = Quaternion.identity; // Reset rotation to 0,0,0
        }
    }

    /// <summary>
    /// Removes a player’s team assignment from the dictionary.
    /// </summary>
    public void RemovePlayerTeam(ulong clientId)
    {
        if (playerTeams.Remove(clientId))
        {
            Debug.Log($"Removed team data for client {clientId}");
        }
    }

    /// <summary>
    /// Public entry point for clients to request a tank change.
    /// Forwards the request to the server.
    /// </summary>
    public void RequestTankChange(string tankName)
    {
        RequestTankChangeServerRpc(tankName);
    }

    /// <summary>
    /// Handles a client’s tank change request on the server.
    /// Applies the change and broadcasts it to all clients.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void RequestTankChangeServerRpc(string tankName, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        // Apply tank change on server
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            var player = client.PlayerObject.GetComponent<Player>();
            if (player != null)
            {
                player.ChangeTank(tankName);
            }
            else
            {
                Debug.LogWarning($"Player object missing Player script for client {clientId}");
            }
            if (tankName != null || tankName != "")
            {
                client.PlayerObject.GetComponent<TankMovement>().SetCanMoveOnServer(true);
                //client.PlayerObject.GetComponent<TankCombat>().currentHealth = client.PlayerObject.GetComponent<TankCombat>().maxHealth;
                client.PlayerObject.GetComponent<TankCombat>().ResetHealth();
            }
        }

        // Broadcast to all clients (including requester)
        UpdateTankClientRpc(clientId, tankName);
    }

    /// <summary>
    /// Called on clients to apply tank change for the specified player.
    /// </summary>
    [ClientRpc]
    private void UpdateTankClientRpc(ulong clientId, string tankName)
    {
        // Ignore on server
        if (IsServer) return;

        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            var localPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject()?.GetComponent<Player>();
            if (localPlayer != null)
            {
                localPlayer.ChangeTank(tankName);
            }
        }
        else
        {
            // Apply to other clients observing this player
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            {
                var player = client.PlayerObject.GetComponent<Player>();
                if (player != null)
                {
                    player.ChangeTank(tankName);
                }
            }
        }
    }

    /// <summary>
    /// Reports a player death on the server and initiates respawn logic.
    /// </summary>
    /// <param name="clientId">Client ID of the dead player.</param>
    public void ReportPlayerDeath(ulong clientId)
    {
        Debug.Log("Server informing all respawn managers of players death");
        Debug.Log($"[Server] Player {clientId} has died.");

        // Inform all clients
        InformPlayersOfDeathClientRpc(clientId);

        // Remove the player's tank or disable control
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            var player = client.PlayerObject.GetComponent<Player>();
            if (player != null)
            {
                player.ChangeTank((TankVarients)null);
            }

            var tankMovement = client.PlayerObject.GetComponent<TankMovement>();
            if (tankMovement != null)
            {
                tankMovement.SetCanMoveOnServer(false);
            }
        }
    }

    /// <summary>
    /// Notifies all clients about a player's death so they can update UI and state.
    /// </summary>
    /// <param name="deadClientId">Client ID of the dead player.</param>
    [ClientRpc]
    private void InformPlayersOfDeathClientRpc(ulong deadClientId)
    {
        Debug.Log("Respawn manager informed of player death by server");
        Debug.Log($"[Client] Player {deadClientId} has died.");

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(deadClientId, out var client))
        {
            var player = client.PlayerObject.GetComponent<Player>();
            if (player != null)
            {
                player.ChangeTank((TankVarients)null);
            }
        }

        if (NetworkManager.Singleton.LocalClientId == deadClientId)
        {
            // This is the player who died
            HUDUI.Singleton?.ShowTankSelectionUI();
        }
    }
}
