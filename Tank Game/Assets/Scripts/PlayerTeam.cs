using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public enum Team
{
    None,
    Blue,
    Orange
}

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

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            networkTeam.OnValueChanged -= OnTeamChanged;
        }
    }

    private void OnTeamChanged(Team previous, Team current)
    {
        team = current;
        UpdateHealthColour();
    }

    public Team team = Team.None;

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

    public static Team GetOppositeTeam(Team team)
    {
        return team switch
        {
            Team.Blue => Team.Orange,
            Team.Orange => Team.Blue,
            _ => Team.None,
        };
    }

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

    static readonly Color32 GREYCOLOR = new Color32(246, 246, 246, 255);
    static readonly Color32 BLUECOLOR = new Color32(72, 187, 255, 255);
    static readonly Color32 ORANGECOLOR = new Color32(255, 125, 10, 255);
    public static Color32 GetNormalColour(Team team)
    {
        return team switch
        {
            Team.Blue => BLUECOLOR,
            Team.Orange => ORANGECOLOR,
            _ => GREYCOLOR,
        };
    }

    static readonly Color32 GREYCOLORARKENED = new Color32(123, 123, 123, 255);
    static readonly Color32 BLUECOLORDARKENED = new Color32(36, 93, 127, 255);
    static readonly Color32 ORANGECOLORDARKENED = new Color32(127, 62, 5, 255);
    public static Color32 GetDarkerColour(Team team)
    {
        return team switch
        {
            Team.Blue => BLUECOLORDARKENED,
            Team.Orange => ORANGECOLORDARKENED,
            _ => GREYCOLORARKENED,
        };
    }

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

    public void AddMinimapIcon()
    {
        if (IsServer) return;

        minimapIconCurrent = Instantiate(minimapIconPrefab, transform);
        minimapIconCurrent.GetComponentInChildren<RawImage>().color = GetNormalColour(team);
    }
}
