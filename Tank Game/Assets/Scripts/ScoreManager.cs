using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Singleton { get; private set; }
    
    // Networked player scores (per clientId)
    private Dictionary<ulong, NetworkVariable<int>> playerScores = new();
    // Networked team scores
    private Dictionary<Team, NetworkVariable<int>> teamScores = new();
    
    [SerializeField] float maxScoreDifference = 100f;

    void Awake()
    {
        Singleton = this;
    }
    void Start()
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
            Debug.Log("Score Manager spawned on network!");
        }
    }

    public float CalculateScoreForTeam(Team team)
    {
        if(team == Team.None) 
        {
            Debug.LogError("Cannot have team score for Team.None...");
            return -1f;
        } 
        Team other = PlayerTeam.GetOppositeTeam(team);

        int thisTeamScore = GetTeamScore(team);
        int otherTeamScore = GetTeamScore(other);

        float scoreDifference = Mathf.Clamp(thisTeamScore - otherTeamScore, -maxScoreDifference, maxScoreDifference);
        float score = (scoreDifference / (2f * maxScoreDifference)) + 0.5f;

        return score;
    }

    

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Initialize team scores
            foreach (Team team in System.Enum.GetValues(typeof(Team)))
            {
                teamScores[team] = new NetworkVariable<int>(0);
            }
        }
    }

    /// <summary>
    /// Call this from the server to update a team's score and the player responsible.
    /// </summary>
    /// <param name="clientId">The client who scored.</param>
    /// <param name="team">The team the player is on.</param>
    /// <param name="scoreDelta">The amount to increase (or decrease) the score.</param>
    public void UpdateScore(ulong clientId, Team team, int scoreDelta)
    {
        if (!IsServer)
            return;

        // Update player score
        if (!playerScores.ContainsKey(clientId))
        {
            playerScores[clientId] = new NetworkVariable<int>(0);
        }

        playerScores[clientId].Value += scoreDelta;

        // Update team score
        if (!teamScores.ContainsKey(team))
        {
            teamScores[team] = new NetworkVariable<int>(0);
        }

        teamScores[team].Value += scoreDelta;

        // Notify all clients
        OnScoreUpdatedClientRpc();
    }

    /// <summary>
    /// Called on all clients when a score is updated.
    /// </summary>
    [ClientRpc]
    private void OnScoreUpdatedClientRpc()
    {
        if (IsServer) { return; } // code doesnt run on server

        switch (GameManager.Singleton.GetCurrentGamemode())
        {
            case GameMode.CaptureTheFlag:
            case GameMode.TeamDeathmatch: // capflg and tdm are the same in terms of score system
                {
                    NetworkObject PlayerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                    Team myTeam = PlayerObject.GetComponent<PlayerTeam>().team;
                    float score = CalculateScoreForTeam(myTeam);
                    HUDUI.Singleton.UpdateBattleStatus(score);
                    break;
                }
            case GameMode.FreeForAll:
                {
                    Debug.LogWarning("FreeForAll NOT yet implemented ...");
                    break;
                }
            default:
                {
                    Debug.LogError("GameMode NOT found...");
                    break;
                }
        }
    }

    // Optional: public accessors for reading scores (server-side only)
    public int GetPlayerScore(ulong clientId)
    {
        if (playerScores.TryGetValue(clientId, out var score))
            return score.Value;

        return 0;
    }

    public int GetTeamScore(Team team)
    {
        if (teamScores.TryGetValue(team, out var score))
            return score.Value;

        return 0;
    }

    public void SetupScoreSystem(GameMode gameMode)
    {
        switch (gameMode)
        {
            case GameMode.CaptureTheFlag:
                {
                    SetupCaptureTheFlag();
                    break;
                }
            case GameMode.FreeForAll:
                {
                    SetupFreeForAll();
                    break;
                }
            case GameMode.TeamDeathmatch:
            default: // default to team death match
                {
                    SetupTeamDeathMatch();
                    break;
                }
        }
    }

    void SetupFreeForAll()
    {
        // remove score on HUD and instead add a top 3
        // currently not supported
        // TODO: add support for free for all
        Debug.LogError("Gamemode: Free For All is NOT currently supported...");
    }
    void SetupCaptureTheFlag()
    {
        // Get all flags from the MAP
        // if no flags then error out
        // if flags are not odd (1, 3, 5, etc.) then error out
        // set flags to no team (default state)
        // setup points for each team
    }
    void SetupTeamDeathMatch()
    {
        // Get all flags (if any) from map and disable them
        // setup points for each team
    }
}
