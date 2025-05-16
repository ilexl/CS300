using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerRespawn : NetworkBehaviour
{
    public static PlayerRespawn Singleton { get; private set; }

    [SerializeField] GameObject playerPrefab;
    [SerializeField] TankVarients tempTankRespawn; // TODO: add tank selection
    [SerializeField] Transform TeamOrangeRespawn, TeamBlueRespawn;
    [SerializeField] float spawnRadius = 5f;

    private Dictionary<ulong, Team> playerTeams = new();

    private void Awake()
    {
        Singleton = this;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SelectTeamServerRpc(Team team, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        Debug.Log($"Spawning Player {clientId}");

        if (playerTeams.ContainsKey(clientId))
        {
            Debug.LogWarning($"Client {clientId} already selected a team.");
            return;
        }

        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId) &&
            NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject != null)
        {
            Debug.LogWarning($"Client {clientId} already has a player object.");
            return;
        }

        playerTeams[clientId] = team;
        Debug.Log($"Client {clientId} joined {team} team");

        SpawnPlayer(clientId, team);
    }

    private void SpawnPlayer(ulong clientId, Team team)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab not assigned!");
            return;
        }

        Vector3 spawnPos = GetRandomizedSpawnPosition(team);
        GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

        NetworkObject netObj = player.GetComponent<NetworkObject>();
        netObj.SpawnAsPlayerObject(clientId);
        Debug.Log("Player spawned!");

        /*netObj.GetComponent<Player>().ChangeTankClientRpc(tempTankRespawn.tankName);

        var pt = player.GetComponent<PlayerTeam>();
        if (pt != null)
        {
            pt.SetTeamSide(team);
        }*/
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

        // Add random offset within a radius
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 randomOffset = new Vector3(randomCircle.x, 0f, randomCircle.y);
        return baseSpawn.position + randomOffset;
    }

    public void RemovePlayerTeam(ulong clientId)
    {
        if (playerTeams.Remove(clientId))
        {
            Debug.Log($"Removed team data for client {clientId}");
        }
    }

    public void Start()
    {
        StartCoroutine(Spawn()); 
    }

    System.Collections.IEnumerator Spawn()
    {
        while (NetworkManager.Singleton == null)
        {
            Debug.Log("Waiting for NetworkManager to be initialized...");
            yield return null; // Wait one frame
        }

        Debug.Log("NetworkManager found, creating player respawn system");
        if (IsServer)
        {
            NetworkObject no = GetComponent<NetworkObject>();
            no.Spawn(true);
        }
    }
}
