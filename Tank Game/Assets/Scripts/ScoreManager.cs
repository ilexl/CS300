using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Singleton { get; private set; }

    public NetworkVariable<int> BlueTeamScore = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> OrangeTeamScore = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] List<CaptureFlag> flags = new();
    private float timer;
    private const float timerReset = 5f;
    void Start()
    {
        Singleton = this;    
    }

    private void Update()
    {
        if (!IsServer) { return; } // only run on the server
        timer -= Time.deltaTime;

        if(timer <= 0f)
        {
            timer = timerReset;
            // Change score

            foreach (CaptureFlag flag in flags)
            {
                AddTeamScore(flag.currentTeam, 1);
            }
        }
    }

    void Awake()
    {
        Singleton = this;
        timer = timerReset;
    }

    int GetTeamScore(Team team)
    {
        return team switch
        {
            Team.Orange => OrangeTeamScore.Value,
            Team.Blue => BlueTeamScore.Value,
            _ => 0
        };
    }

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

    public override void OnNetworkDespawn()
    {
        OrangeTeamScore.OnValueChanged -= HandleAnyTeamScoreChanged;
        BlueTeamScore.OnValueChanged -= HandleAnyTeamScoreChanged;
    }

    void ResetScores()
    {
        if (!IsServer) return;

        OrangeTeamScore.Value = 0;
        BlueTeamScore.Value = 0;
    }

    private void HandleAnyTeamScoreChanged(int oldValue, int newValue)
    {
        if (IsServer) { return; }
        NetworkObject localPlayerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        Team localTeam = localPlayerObject.GetComponent<PlayerTeam>().team;

        if(localTeam == Team.None) return; // player doesnt have a team yet

        Debug.Log($"The score is currently Blue {BlueTeamScore.Value} : Orange {OrangeTeamScore.Value}");

        float localScore = CalculateScoreForTeam(localTeam); // between 0f and 1f
        HUDUI.Singleton.UpdateScore(localScore);
    }

    public void ForceUpdateScoreUI()
    {
        // Just simulate value change to trigger the same logic
        HandleAnyTeamScoreChanged(-1, -1); // values don't matter
    }

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
                    break;
                }
        }
    }

#if UNITY_EDITOR
    public bool EDITOR_IsServer()
    {
        if (EditorApplication.isPlaying is false) { return false; } // cant perform this while not playing
        return IsServer;
    }

    public void EDITOR_ChangeOrangeValue(int value)
    {
        OrangeTeamScore.Value += value;
    }

    public void EDITOR_ChangeBlueValue(int value)
    {
        BlueTeamScore.Value += value;
    }
#endif
}

#if UNITY_EDITOR
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