using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Singleton { get; private set; }
    private Dictionary<ulong, int> playerScores;
    private Dictionary<Team, int> teamScores;

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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        // This will be the start function here
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
