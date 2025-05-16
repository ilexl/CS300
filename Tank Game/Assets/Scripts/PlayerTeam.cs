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

public class PlayerTeam : MonoBehaviour
{
    public Team team = Team.None;
    [SerializeField] GameObject healthBarPrefab;
    public GameObject healthBarCurrent;

    public void Awake()
    {
        if (healthBarPrefab is null)
        {
            // try load from resources
            GameObject load = Resources.Load<GameObject>("PlayerSetup/HealthBar");
            if (load != null)
            {
                healthBarPrefab = load;
                return;
            }
            Debug.LogError("Health Bar Prefab not found...");
        }
    }

    public void SetTeamSide(Team side)
    {
        team = side;
        Debug.Log($"Player set to team {team}");
        UpdateHealthColour();
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
        if (s == null) return; // Will be null on server

        if (max != 0) s.maxValue = max;
        s.minValue = 0;
        s.value = current;
    }

    private void UpdateHealthColour()
    {
        // Prevent running in edit mode with invalid scene state
        if (!Application.isPlaying) return;

        if (healthBarCurrent != null)
        {
#if UNITY_EDITOR
            DestroyImmediate(healthBarCurrent);
#else
            Destroy(healthBarCurrent);
#endif
        }

        // Ensure parent is a scene object
        if (gameObject.scene.name == null)
        {
            Debug.LogWarning("Skipping instantiation because GameObject is not in a scene.");
            return;
        }

        // Only instantiate under valid runtime conditions
        healthBarCurrent = Instantiate(healthBarPrefab);
        healthBarCurrent.transform.SetParent(transform, false); // Avoid parenting errors

        var images = healthBarCurrent.GetComponentsInChildren<Image>();
        foreach (Image image in images)
        {
            if (image.gameObject.name == "Fill")
            {
                Debug.Log("Normal Color Set");
                image.color = GetNormalColour(team);
            }
            if (image.gameObject.name == "Background")
            {
                Debug.Log("Dark Color Set");
                image.color = GetDarkerColour(team);
            }
        }

        healthBarCurrent.SetActive(!GetComponent<Player>().LocalPlayer);

        if (GetComponent<Player>().LocalPlayer)
        {
            if (HUDUI.current == null) return;
            HUDUI.current.UpdateTeamColour(team);
            Debug.Log("Updated HUD Team Colour");
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
            if (!Application.isPlaying) return; // Prevent UpdateHealthColour() logic in edit-time
            SetTeamSide(team);
        };
#endif
    }
}
