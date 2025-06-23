using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Manages the scoring system for both teams, updates scores based on flag captures,
/// and handles game over conditions in a networked multiplayer game.
/// </summary>
public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Singleton { get; private set; } // Singleton instance for global access.

    // Networked score variable for the Blue team.
    public NetworkVariable<int> BlueTeamScore = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    // Networked score variable for the Orange team.
    public NetworkVariable<int> OrangeTeamScore = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // List of flags to track for scoring.
    [SerializeField] List<CaptureFlag> flags = new();

    private float timer;
    private const float timerReset = 5f;

    
    /// <summary>
    /// Ensures singleton is assigned on Start.
    /// </summary>
    void Start()
    {
        Singleton = this;    
    }

    /// <summary>
    /// Initializes singleton and timer.
    /// </summary>
    void Awake()
    {
        Singleton = this;
        timer = timerReset;
    }

    /// <summary>
    /// Server-only timer that periodically adds score based on flag control.
    /// </summary>
    private void Update()
    {
        if (!IsServer) { return; } // Only run logic on server
        timer -= Time.deltaTime;

        if(timer <= 0f)
        {
            timer = timerReset;

            foreach (CaptureFlag flag in flags)
            {
                AddTeamScore(flag.currentTeam, 1);
            }
        }
    }


    

    /// <summary>
    /// Gets the current score for the specified team.
    /// </summary>
    /// <param name="team">Team to query score for.</param>
    /// <returns>Current score for the team.</returns>
    int GetTeamScore(Team team)
    {
        return team switch
        {
            Team.Orange => OrangeTeamScore.Value,
            Team.Blue => BlueTeamScore.Value,
            _ => 0
        };
    }

    /// <summary>
    /// Calculates a normalized score ratio for a team compared to its opponent.
    /// Returns a value between 0 and 1 indicating relative standing.
    /// </summary>
    /// <param name="team">Team to calculate score for.</param>
    /// <returns>Normalized score value.</returns>
    public float CalculateScoreForTeam(Team team)
    {
        int thisTeamScore = GetTeamScore(team);
        int otherTeamScore = GetTeamScore(PlayerTeam.GetOppositeTeam(team));

        if (team == Team.None)
        {
            Debug.LogError("Cannot have team score for Team.None...");
            return -1f;
        }

        float scoreDifference = Mathf.Clamp(thisTeamScore - otherTeamScore, -100, 100);
        float score = (scoreDifference / (2f * 100)) + 0.5f;

        return score;
    }

    /// <summary>
    /// Called when the network object spawns.
    /// Subscribes to score change events and resets scores if server.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        // Subscribe both to a shared handler
        OrangeTeamScore.OnValueChanged += HandleAnyTeamScoreChanged;
        BlueTeamScore.OnValueChanged += HandleAnyTeamScoreChanged;
        
        if (IsServer)
        {
            ResetScores();
        }

    }

    /// <summary>
    /// Called when the network object despawns.
    /// Unsubscribes from score change events.
    /// </summary>
    public override void OnNetworkDespawn()
    {
        OrangeTeamScore.OnValueChanged -= HandleAnyTeamScoreChanged;
        BlueTeamScore.OnValueChanged -= HandleAnyTeamScoreChanged;
    }

    /// <summary>
    /// Resets both teams' scores to zero. Server-only operation.
    /// </summary>
    void ResetScores()
    {
        if (!IsServer) return;

        OrangeTeamScore.Value = 0;
        BlueTeamScore.Value = 0;
    }

    /// <summary>
    /// Handles updates when either team’s score changes.
    /// Updates client UI or checks for game over on server.
    /// </summary>
    /// <param name="oldValue">Previous score value (unused).</param>
    /// <param name="newValue">New score value (unused).</param>
    private void HandleAnyTeamScoreChanged(int oldValue, int newValue)
    {
        if (IsServer) 
        {
            CheckIfGameOver();
            return; 
        }

        NetworkObject localPlayerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        Team localTeam = localPlayerObject.GetComponent<PlayerTeam>().team;

        if(localTeam == Team.None) return; // Player not assigned a team yet

        Debug.Log($"The score is currently Blue {BlueTeamScore.Value} : Orange {OrangeTeamScore.Value}");

        float localScore = CalculateScoreForTeam(localTeam); // Normalized 0 to 1
        HUDUI.Singleton.UpdateScore(localScore);
    }

    /// <summary>
    /// Checks if the game over condition is met (score difference of 100).
    /// Triggers game over event and shuts down the server.
    /// </summary>
    private void CheckIfGameOver()
    {
        if(!IsServer) return;
        int scoreDifference = Mathf.Abs(Mathf.Clamp(BlueTeamScore.Value - OrangeTeamScore.Value, -100, 100));
        if(scoreDifference == 100)
        {
            Team leadingTeam = BlueTeamScore.Value >= OrangeTeamScore.Value ? Team.Blue : Team.Orange;
            GameOverClientRpc(leadingTeam);
            #if UNITY_EDITOR
            Debug.Log("Server Shutting Down!");
            NetworkManager.Singleton.Shutdown();
            #else
            NetworkManager.Singleton.Shutdown();
            Application.Quit();
            #endif
        }
    }

    /// <summary>
    /// Notifies clients that the game has ended and shows the game over UI.
    /// </summary>
    /// <param name="winner">The winning team.</param>
    [ClientRpc]
    void GameOverClientRpc(Team winner)
    {
        if (IsServer) { return; }
        Debug.Log("Game has ended!");
        // code here for client
        HUDUI.Singleton.ShowGameOver(winner);
    }

    /// <summary>
    /// Forces a UI update by simulating a score value change.
    /// </summary>
    public void ForceUpdateScoreUI()
    {
        HandleAnyTeamScoreChanged(-1, -1); // Values are irrelevant
    }

    /// <summary>
    /// Adds a value to the specified team's score.
    /// </summary>
    /// <param name="team">Team to add score for.</param>
    /// <param name="value">Amount to add.</param>
    void AddTeamScore(Team team, int value)
    {
        switch (team)
        {
            case Team.Blue:
                {
                    BlueTeamScore.Value += value;
                    break;
                }
            case Team.Orange:
                {
                    OrangeTeamScore.Value += value;
                    break;
                }
            case Team.None:
            default:
                {
                    // Ignore invalid team values
                    break;
                }
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Checks if running as server in the Unity Editor.
    /// </summary>
    /// <returns>True if server; otherwise false.</returns>
    public bool EDITOR_IsServer()
    {
        if (EditorApplication.isPlaying is false) { return false; } // Only valid during play mode
        return IsServer;
    }

    /// <summary>
    /// Editor helper to change the Orange team score.
    /// </summary>
    /// <param name="value">Amount to add or subtract.</param>
    public void EDITOR_ChangeOrangeValue(int value)
    {
        OrangeTeamScore.Value += value;
    }

    /// <summary>
    /// Editor helper to change the Blue team score.
    /// </summary>
    /// <param name="value">Amount to add or subtract.</param>
    public void EDITOR_ChangeBlueValue(int value)
    {
        BlueTeamScore.Value += value;
    }
#endif
}

#if UNITY_EDITOR
/// <summary>
/// Custom inspector to allow score adjustments in the Unity Editor for testing.
/// </summary>
[CustomEditor(typeof(ScoreManager))]
public class EDITOR_ScoreManager : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ScoreManager sm = (ScoreManager)target;
        if (sm.EDITOR_IsServer())
        {
            if (GUILayout.Button("+1 to Blue"))
            {
                sm.EDITOR_ChangeBlueValue(1);
            }
            if (GUILayout.Button("-1 to Blue"))
            {
                sm.EDITOR_ChangeBlueValue(-1);
            }
            if (GUILayout.Button("+5 to Blue"))
            {
                sm.EDITOR_ChangeBlueValue(5);
            }
            if (GUILayout.Button("-5 to Blue"))
            {
                sm.EDITOR_ChangeBlueValue(-5);
            }

            GUILayout.Space(20);

            if (GUILayout.Button("+1 to Orange"))
            {
                sm.EDITOR_ChangeOrangeValue(1);

            }
            if (GUILayout.Button("-1 to Orange"))
            {
                sm.EDITOR_ChangeOrangeValue(-1);
            }
            if (GUILayout.Button("+5 to Orange"))
            {
                sm.EDITOR_ChangeOrangeValue(5);
            }
            if (GUILayout.Button("-5 to Orange"))
            {
                sm.EDITOR_ChangeOrangeValue(-5);
            }


        }

        
    }
}
#endif