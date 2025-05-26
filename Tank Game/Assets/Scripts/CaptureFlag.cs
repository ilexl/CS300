using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System;

public class CaptureFlag : NetworkBehaviour
{
    public string FlagLetter;

    [SerializeField] private Vector3 flagUpPos;
    [SerializeField] private Vector3 flagDownPos;
    [SerializeField] private GameObject flagObj;
    [SerializeField] private UnityEngine.Material whiteMat, teamOrange, teamBlue;

    [SerializeField] private float captureDuration = 5f;
    private float captureProgress = 0f;

    [SerializeField] private NetworkVariable<Team> owningTeam = new NetworkVariable<Team>(Team.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private NetworkVariable<float> flagLerp = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private Team capturingTeam = Team.None;

    [SerializeField] private readonly HashSet<PlayerTeam> playersInZone = new HashSet<PlayerTeam>();

    private float targetLerp = 0f; // NEW: used to smooth flag movement
    public GameObject flagUI;

    private void Start()
    {
        StartCoroutine(WaitForNetwork());
    }

    public Team currentTeam => owningTeam.Value;

    private System.Collections.IEnumerator WaitForNetwork()
    {
        while (NetworkManager.Singleton == null)
        {
            Debug.Log("Waiting for NetworkManager...");
            yield return null;
        }

        if (IsServer)
        {
            if (GetComponent<NetworkObject>().IsSpawned == false) { GetComponent<NetworkObject>().Spawn(true); }

            Debug.Log("Flag spawned on network!");
            ResetFlag();
        }

        owningTeam.OnValueChanged += OnTeamChanged;
        
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            flagUI = HUDUI.Singleton.CreateFlagUI(FlagLetter);
            flagLerp.OnValueChanged += UpdateUIVisual;
        }
    }

    private void UpdateUIVisual(float previousValue, float newValue)
    {
        if (flagUI == null || HUDUI.Singleton == null)
            return;

        Team displayTeam;
        float progress = 1 - newValue;

        if (capturingTeam != Team.None && newValue > previousValue)
        {
            // Capturing in progress (flag moving down)
            displayTeam = capturingTeam;
        }
        else
        {
            // Reverting back up (flag moving up)
            displayTeam = owningTeam.Value;
        }

        HUDUI.Singleton.UpdateFlagUIValues(flagUI, displayTeam, progress);
    }

    public void ResetFlag()
    {
        owningTeam.Value = Team.None;
        capturingTeam = Team.None;
        captureProgress = 0f;
        flagLerp.Value = 0f;
        targetLerp = 0f;
        UpdateFlagVisual(Team.None);
    }

    private void Update()
    {
        if (IsClient)
        {
            InterpolateFlagPosition(flagLerp.Value);
        }

        if (IsServer)
        {
            Team teamInZone = GetSingleTeamInZone();

            if (teamInZone != Team.None && teamInZone != owningTeam.Value)
            {
                capturingTeam = teamInZone;
                captureProgress += Time.deltaTime;
                targetLerp = Mathf.Clamp01(captureProgress / captureDuration);

                if (captureProgress >= captureDuration)
                {
                    owningTeam.Value = capturingTeam;
                    captureProgress = 0f;
                    targetLerp = 0f;
                }
            }
            else
            {
                // contested or nobody in zone
                captureProgress = Mathf.MoveTowards(captureProgress, 0f, Time.deltaTime);
                targetLerp = Mathf.Clamp01(captureProgress / captureDuration);
            }

            // Smoothly move flagLerp.Value toward targetLerp over time
            flagLerp.Value = Mathf.MoveTowards(flagLerp.Value, targetLerp, Time.deltaTime / captureDuration);
        }
    }

    private void InterpolateFlagPosition(float lerp)
    {
        float y = Mathf.Lerp(flagUpPos.y, flagDownPos.y, lerp);
        Vector3 pos = flagObj.transform.position;
        pos.y = y;
        flagObj.transform.position = pos;
    }

    private void OnTeamChanged(Team previous, Team current)
    {
        UpdateFlagVisual(current);
    }

    private void UpdateFlagVisual(Team team)
    {
        if (IsServer) return; // Prevent server from updating materials

        UnityEngine.Material mat = whiteMat;
        if (team == Team.Orange) mat = teamOrange;
        else if (team == Team.Blue) mat = teamBlue;

        var mr = flagObj.GetComponent<MeshRenderer>();
        if (mr != null) { mr.material = mat; }
    }

    private Team GetSingleTeamInZone()
    {
        bool hasOrange = false;
        bool hasBlue = false;

        foreach (PlayerTeam pt in playersInZone)
        {
            if (pt == null) continue;
            if (pt.team == Team.Orange) hasOrange = true;
            if (pt.team == Team.Blue) hasBlue = true;
        }

        if (hasOrange && hasBlue) return Team.None;
        if (hasOrange) return Team.Orange;
        if (hasBlue) return Team.Blue;
        return Team.None;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        PlayerTeam player = other.GetComponent<PlayerTeam>();
        if (player != null && !playersInZone.Contains(player))
        {
            Debug.Log($"{player} from team {player.team} entered flag zone");
            playersInZone.Add(player);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        PlayerTeam player = other.GetComponent<PlayerTeam>();
        if (player != null && playersInZone.Contains(player))
        {
            Debug.Log("{player} left flag zone");
            playersInZone.Remove(player);
        }
    }
}
