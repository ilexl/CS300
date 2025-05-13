using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTeam : MonoBehaviour
{
    public Side team = Side.None;
    [SerializeField] GameObject healthBarPrefab;
    public GameObject healthBarCurrent;

    [System.Serializable]
    public enum Side
    {
        None,
        Blue,
        Orange
    }

    public void SetTeamSide(Side side)
    {
        team = side;
        Debug.Log($"Player set to team {team}");
        UpdateHealthColour();
    }

    public void UpdateHealthBar(float current, float max = 0)
    {
        if(healthBarCurrent is null || healthBarCurrent == null) { UpdateHealthColour(); }
        if(healthBarCurrent is null || healthBarCurrent == null) { Debug.LogError("Health bar not being added correctly..."); }
        Slider s = healthBarCurrent.GetComponentInChildren<Slider>();
        
        if(max is not 0) { s.maxValue = max; }
        s.minValue = 0; // min value is ALWAYS zero
        s.value = current;
    }

    private void UpdateHealthColour()
    {
        if(healthBarCurrent is not null)
        {
            #if UNITY_EDITOR
            DestroyImmediate(healthBarCurrent);
            #else
            Destroy(healthBarCurrent);
            #endif
        }
        healthBarCurrent = Instantiate(healthBarPrefab, transform);
        var images = healthBarCurrent.GetComponentsInChildren<Image>();
        foreach (Image image in images)
        {
            if(image.gameObject.name == "Fill")
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

        // update UI
        if (GetComponent<Player>().LocalPlayer)
        {
            if (HUDUI.current is null) { return; }
            HUDUI.current.UpdateTeamColour(team);
            Debug.Log("Updated HUD Team Colour");
        }
    }

    static readonly Color32 GREYCOLOR = new Color32(246, 246, 246, 255);
    static readonly Color32 BLUECOLOR = new Color32(72, 187, 255, 255);
    static readonly Color32 ORANGECOLOR = new Color32(255, 125, 10, 255);
    public static Color32 GetNormalColour(Side team)
    {
        switch (team)
        {
            case Side.Blue:
                {
                    return BLUECOLOR;
                }
            case Side.Orange:
                {
                    return ORANGECOLOR;
                }
            case Side.None:
            default:
                {
                    return GREYCOLOR;
                }
        }
    }

    static readonly Color32 GREYCOLORARKENED = new Color32(123, 123, 123, 255);
    static readonly Color32 BLUECOLORDARKENED = new Color32(36, 93, 127, 255);
    static readonly Color32 ORANGECOLORDARKENED = new Color32(127, 62, 5, 255);
    public static Color32 GetDarkerColour(Side team)
    {
        switch (team)
        {
            case Side.Blue:
                {
                    return BLUECOLORDARKENED;
                }
            case Side.Orange:
                {
                    return ORANGECOLORDARKENED;
                }
            case Side.None:
            default:
                {
                    return GREYCOLORARKENED;
                }
        }
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (EditorApplication.isPlaying is false)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorApplication.isUpdating) return; // Prevents execution during asset imports
            if (BuildPipeline.isBuildingPlayer) return; // Prevents issues during builds
        }

        // Schedule object destruction to avoid Unity serialization issues
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null) return; // Prevent null reference errors if the object was deleted
            SetTeamSide(team);
        };
#endif
    }
}
