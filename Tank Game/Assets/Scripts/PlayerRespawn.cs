using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerRespawn : NetworkBehaviour
{
    public static PlayerRespawn Singleton { get; private set; }

    [SerializeField] TankVarients tempTankRespawn;
    [SerializeField] Transform TeamOrangeRespawn, TeamBlueRespawn;
    [SerializeField] float spawnRadius = 5f;

    private Dictionary<ulong, Team> playerTeams = new();

    private void Awake()
    {
        Singleton = this;
    }

    public void Start()
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
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SelectTeamServerRpc(Team team, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (playerTeams.ContainsKey(clientId))
        {
            Debug.LogWarning($"Client {clientId} already selected a team.");
            return;
        }

        GameObject player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;
        player.transform.position = GetRandomizedSpawnPosition(team);

        playerTeams[clientId] = team;
        Debug.Log($"Client {clientId} joined {team} team");
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
        }

        // Then broadcast to all other clients (we'll handle local below)
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
}
