using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Handles capture-the-flag zone logic for a single flag.
/// Supports multiplayer authority, visual syncing, team ownership, and UI updates.
/// </summary>
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
    [SerializeField] private TextMeshProUGUI minimapLetter;
    [SerializeField] private RawImage minimapCircle;

    private float targetLerp = 0f; // NEW: used to smooth flag movement
    public GameObject flagUI;

    /// <summary>
    /// Shortcut for accessing the flag’s current owning team.
    /// </summary>
    public Team currentTeam => owningTeam.Value;

    /// <summary>
    /// Waits for the network to be available before proceeding with server-specific logic.
    /// Ensures the flag is spawned and initialized on the server.
    /// </summary>
    private void Start()
    {
        StartCoroutine(WaitForNetwork());
    }

    /// <summary>
    /// Waits for the NetworkManager, then spawns and initializes the flag object if on server.
    /// </summary>
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

    /// <summary>
    /// Client-side initialization: sets up HUD UI and registers value change listeners.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            flagUI = HUDUI.Singleton.CreateFlagUI(FlagLetter);
            flagLerp.OnValueChanged += UpdateUIVisual;
        }
    }

    /// <summary>
    /// Updates the HUD UI and minimap to reflect flag capture progress and controlling team.
    /// </summary>
    private void UpdateUIVisual(float previousValue, float newValue)
    {
        if (flagUI == null || HUDUI.Singleton == null)
            return;

        Team displayTeam;
        float progress = 1 - newValue;

        if (capturingTeam != Team.None && newValue > previousValue)
        {
            displayTeam = capturingTeam; // Currently being captured
        }
        else
        {
            displayTeam = owningTeam.Value;  // Reverting or neutral
        }

        HUDUI.Singleton.UpdateFlagUIValues(flagUI, displayTeam, progress);

        if(minimapLetter == null || minimapCircle == null) { return; }
        minimapLetter.text = FlagLetter;
        minimapLetter.color = PlayerTeam.GetNormalColour(displayTeam);
        minimapCircle.color = PlayerTeam.GetNormalColour(displayTeam);
    }

    /// <summary>
    /// Resets the flag state to neutral, resetting visuals and ownership.
    /// </summary>
    public void ResetFlag()
    {
        owningTeam.Value = Team.None;
        capturingTeam = Team.None;
        captureProgress = 0f;
        flagLerp.Value = 0f;
        targetLerp = 0f;
        UpdateFlagVisual(Team.None);
    }

    /// <summary>
    /// Handles interpolation (client-side) and flag ownership/capture logic (server-side).
    /// </summary>
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
                // Start or continue capturing the flag
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
                // Contested or empty — revert progress
                captureProgress = Mathf.MoveTowards(captureProgress, 0f, Time.deltaTime);
                targetLerp = Mathf.Clamp01(captureProgress / captureDuration);
            }

            // Smoothly animate flag progress value (for visual sync)
            flagLerp.Value = Mathf.MoveTowards(flagLerp.Value, targetLerp, Time.deltaTime / captureDuration);
        }
    }

    /// <summary>
    /// Smoothly adjusts the flag model's height based on lerp progress.
    /// </summary>
    private void InterpolateFlagPosition(float lerp)
    {
        float y = Mathf.Lerp(flagUpPos.y, flagDownPos.y, lerp);
        Vector3 pos = flagObj.transform.localPosition;
        pos.y = y / transform.localScale.y;
        flagObj.transform.localPosition = pos;
    }

    /// <summary>
    /// Called on team ownership change — updates flag color (clients only).
    /// </summary>
    private void OnTeamChanged(Team previous, Team current)
    {
        UpdateFlagVisual(current);
    }

    /// <summary>
    /// Updates the flag mesh material based on team ownership.
    /// </summary>
    private void UpdateFlagVisual(Team team)
    {
        if (IsServer) return; // Server doesn't handle visuals

        UnityEngine.Material mat = whiteMat;
        if (team == Team.Orange) mat = teamOrange;
        else if (team == Team.Blue) mat = teamBlue;

        var mr = flagObj.GetComponent<MeshRenderer>();
        if (mr != null) { mr.material = mat; }
    }

    /// <summary>
    /// Returns the single team present in the zone, or None if contested or empty.
    /// </summary>
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

    /// <summary>
    /// Called when a player enters the flag zone.
    /// Adds them to the active capture set if on the server.
    /// </summary>
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

    /// <summary>
    /// Called when a player exits the flag zone.
    /// Removes them from the active capture set if on the server.
    /// </summary>
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
