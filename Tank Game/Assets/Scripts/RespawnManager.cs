using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RespawnManager : NetworkBehaviour
{
    public static RespawnManager Singleton { get; private set; }

    [SerializeField] TankVarients tempTankRespawn;
    [SerializeField] Transform TeamOrangeRespawn, TeamBlueRespawn;
    [SerializeField] float spawnRadius = 5f;

    private Dictionary<ulong, Team> playerTeams = new();

    void Awake()
    {
        Singleton = this;
    }

    void Start()
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
            Debug.Log("Respawn Manager spawned on network!");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SelectTeamServerRpc(Team team, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (playerTeams.ContainsKey(clientId))
        {
            if (playerTeams[clientId] == team)
            {
                Debug.LogWarning($"Client {clientId} already in the team.");
                return;
            }
            else
            {
                RemovePlayerTeam(clientId);
            }
        }

        playerTeams[clientId] = team;
        Debug.Log($"Client {clientId} joined {team} team");

        Vector3 spawnPos = GetRandomizedSpawnPosition(team);
        SendPlayerToSpawnClientRpc(clientId, spawnPos);

        // Notify all clients that this player selected a team
        NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerTeam>().SetTeamSide(team); // set the team
        NotifyTeamSelectedClientRpc(clientId, team);
    }

    [ClientRpc]
    private void NotifyTeamSelectedClientRpc(ulong clientId, Team team)
    {
        if (IsServer) { return; }
        Debug.Log($"Client {clientId} selected team {team}");

        NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerTeam>().SetTeamSide(team); // set the team
    }

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


    public void RemovePlayerTeam(ulong clientId)
    {
        if (playerTeams.Remove(clientId))
        {
            Debug.Log($"Removed team data for client {clientId}");
        }
    }

    public void RequestTankChange(string tankName)
    {
        RequestTankChangeServerRpc(tankName);
    }

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

    [ClientRpc]
    private void UpdateTankClientRpc(ulong clientId, string tankName)
    {
        // Ignore on server
        if (IsServer) return;

        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            // Apply to local player (the requester)
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

    [ServerRpc(RequireOwnership = false)]
    public void ReportPlayerDeathServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
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

    [ClientRpc]
    private void InformPlayersOfDeathClientRpc(ulong deadClientId)
    {
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
            HUDUI.Singleton?.ShowRespawnUI();
        }
    }
}
