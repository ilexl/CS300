using UnityEngine;

/// <summary>
/// Controls the visual state of a tank, including normal visibility,
/// destruction state (placeholder), and sniper mode visibility.
/// </summary>
public class TankVisuals : MonoBehaviour
{
    private MeshRenderer[] meshes;

    /// <summary>
    /// Makes all tank mesh renderers visible and exits sniper mode UI.
    /// </summary>
    public void ShowTankNormal()
    {
        RefreshList();
        if (meshes == null)
        {
            meshes = GetComponentsInChildren<MeshRenderer>();
        }
        if(meshes is null) { return; }
        foreach (var mesh in meshes)
        {
            mesh.enabled = true;
        }
        if (HUDUI.Singleton is not null)
        {
            HUDUI.Singleton.HideSniperMode();
        }
    }

    /// <summary>
    /// Placeholder for handling tank destruction visuals.
    /// </summary>
    public void ShowTankDestroyed()
    {
        Debug.LogWarning("Show Tank Destroyed is NOT implemented yet...");
    }

    /// <summary>
    /// Hides all tank mesh renderers and activates sniper mode UI.
    /// </summary>
    public void HideTankSniperMode()
    {
        RefreshList();
        if (meshes == null)
        {
            meshes = GetComponentsInChildren<MeshRenderer>();
        }
        if (meshes is null) { return; }
        foreach (var mesh in meshes)
        {
            mesh.enabled = false;
        }
        HUDUI.Singleton.ShowSniperMode();
    }

    /// <summary>
    /// Initializes mesh renderer list when the object awakens.
    /// </summary>
    void Awake()
    {
        RefreshList();
    }

    /// <summary>
    /// Ensures mesh renderer list is populated when the object starts.
    /// </summary>
    private void Start()
    {
        RefreshList();
    }

    /// <summary>
    /// Updates the list of mesh renderers attached to the tank.
    /// </summary>
    public void RefreshList()
    {
        meshes = GetComponentsInChildren<MeshRenderer>();
    }
}
