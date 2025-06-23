using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a player's team state (None, Blue, Orange) and handles
/// related visuals like health bars and minimap icons.
/// </summary>
[System.Serializable]
public enum Team
{
    None,
    Blue,
    Orange
}

/// <summary>
/// Manages team assignment, team-colored visuals (health bar, minimap icon),
/// and communicates team information across the network.
/// </summary>
public class PlayerTeam : NetworkBehaviour
{
    private NetworkVariable<Team> networkTeam = new NetworkVariable<Team>(
        Team.None,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [SerializeField] private GameObject healthBarPrefab;
    public GameObject healthBarCurrent;

    [SerializeField] private GameObject minimapIconPrefab;
    public GameObject minimapIconCurrent;

    public Team team = Team.None;

    /// <summary>
    /// Called when the object is spawned on the network.
    /// Initializes team color and subscribes to team updates if client.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            networkTeam.OnValueChanged += OnTeamChanged;
            team = networkTeam.Value;
            UpdateHealthColour();
        }

        if (IsServer)
        {
            team = networkTeam.Value;
        }
    }

    /// <summary>
    /// Called when the object is despawned from the network.
    /// Unsubscribes from the team change event.
    /// </summary>
    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            networkTeam.OnValueChanged -= OnTeamChanged;
        }
    }

    /// <summary>
    /// Event callback for when the team value changes on the network.
    /// Updates internal team value and health bar color.
    /// </summary>
    private void OnTeamChanged(Team previous, Team current)
    {
        team = current;
        UpdateHealthColour();
    }

    /// <summary>
    /// Unity lifecycle method for initialization before Start.
    /// Attempts to load the health bar prefab from Resources if not set.
    /// </summary>
    private void Awake()
    {
        if (healthBarPrefab == null)
        {
            GameObject load = Resources.Load<GameObject>("PlayerSetup/HealthBar");
            if (load != null)
            {
                healthBarPrefab = load;
                return;
            }
            Debug.LogError("Health Bar Prefab not found...");
        }
    }

    /// <summary>
    /// Assigns a team to the player (server-side) and updates all associated UI and visual elements.
    /// </summary>
    public void SetTeamSide(Team newTeam)
    {
        if (IsServer)
        {
            networkTeam.Value = newTeam;
        }

        team = newTeam;
        Debug.Log($"Player set to team {team}");
        UpdateHealthColour();

        if (!IsOwner || GameManager.Singleton == null || HUDUI.Singleton == null)
            return;

        if (GameManager.Singleton.GetCurrentGamemode() == GameMode.CaptureTheFlag ||
            GameManager.Singleton.GetCurrentGamemode() == GameMode.TeamDeathmatch)
        {
            if (team != Team.None)
            {
                HUDUI.Singleton.SetTeams(team, GetOppositeTeam(team));
            }
        }

        ScoreManager.Singleton.ForceUpdateScoreUI();
    }

    /// <summary>
    /// Returns the opposite team from the given team.
    /// </summary>
    public static Team GetOppositeTeam(Team team)
    {
        return team switch
        {
            Team.Blue => Team.Orange,
            Team.Orange => Team.Blue,
            _ => Team.None,
        };
    }

    /// <summary>
    /// Updates the health bar value. If max is specified, sets max as well.
    /// </summary>
    public void UpdateHealthBar(float current, float max = 0)
    {
        if (healthBarCurrent == null)
        {
            UpdateHealthColour();
        }

        if (healthBarCurrent == null)
        {
            Debug.LogError("Health bar not being added correctly...");
            return;
        }

        Slider s = healthBarCurrent.GetComponentInChildren<Slider>();
        if (s == null) return;

        if (max != 0) s.maxValue = max;
        s.minValue = 0;
        s.value = current;
    }

    /// <summary>
    /// Destroys and re-creates the health bar object and sets the color based on the current team.
    /// Also updates minimap icon color.
    /// </summary>
    private void UpdateHealthColour()
    {
        if (!Application.isPlaying) return;

        if (healthBarCurrent != null)
        {
#if UNITY_EDITOR
            DestroyImmediate(healthBarCurrent);
#else
            Destroy(healthBarCurrent);
#endif
        }

        if (gameObject.scene.name == null)
        {
            Debug.LogWarning("Skipping instantiation because GameObject is not in a scene.");
            return;
        }

        healthBarCurrent = Instantiate(healthBarPrefab);
        healthBarCurrent.transform.SetParent(transform, false);

        var images = healthBarCurrent.GetComponentsInChildren<Image>();
        foreach (Image image in images)
        {
            if (image.gameObject.name == "Fill")
            {
                image.color = GetNormalColour(team);
            }
            if (image.gameObject.name == "Background")
            {
                image.color = GetDarkerColour(team);
            }
        }

        healthBarCurrent.SetActive(!GetComponent<Player>().LocalPlayer);

        if (GetComponent<Player>().LocalPlayer && HUDUI.Singleton != null)
        {
            HUDUI.Singleton.UpdateTeamColour(team);
        }

        if (minimapIconCurrent != null)
        {
            minimapIconCurrent.GetComponentInChildren<RawImage>().color = GetNormalColour(team);
        }
    }

    // Static colour definitions for consistent team color usage
    static readonly Color32 GREYCOLOR = new Color32(246, 246, 246, 255);
    static readonly Color32 BLUECOLOR = new Color32(72, 187, 255, 255);
    static readonly Color32 ORANGECOLOR = new Color32(255, 125, 10, 255);

    /// <summary>
    /// Returns the bright color representing the given team.
    /// </summary>
    public static Color32 GetNormalColour(Team team)
    {
        return team switch
        {
            Team.Blue => BLUECOLOR,
            Team.Orange => ORANGECOLOR,
            _ => GREYCOLOR,
        };
    }

    // Static colour definitions for consistent team dark color usage
    static readonly Color32 GREYCOLORARKENED = new Color32(123, 123, 123, 255);
    static readonly Color32 BLUECOLORDARKENED = new Color32(36, 93, 127, 255);
    static readonly Color32 ORANGECOLORDARKENED = new Color32(127, 62, 5, 255);

    /// <summary>
    /// Returns the darker color variant of the given team color.
    /// </summary>
    public static Color32 GetDarkerColour(Team team)
    {
        return team switch
        {
            Team.Blue => BLUECOLORDARKENED,
            Team.Orange => ORANGECOLORDARKENED,
            _ => GREYCOLORARKENED,
        };
    }

    /// <summary>
    /// Unity editor-specific method to force team setup when values are changed in editor.
    /// </summary>
    private void OnValidate()
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorApplication.isUpdating) return;
            if (BuildPipeline.isBuildingPlayer) return;
        }

        EditorApplication.delayCall += () =>
        {
            if (this == null) return;
            if (!Application.isPlaying) return;
            SetTeamSide(team);
        };
#endif
    }

    /// <summary>
    /// Instantiates a minimap icon on the client and sets its color based on the team.
    /// </summary>
    public void AddMinimapIcon()
    {
        if (IsServer) return;

        minimapIconCurrent = Instantiate(minimapIconPrefab, transform);
        minimapIconCurrent.GetComponentInChildren<RawImage>().color = GetNormalColour(team);
    }
}
