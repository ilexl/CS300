using UnityEngine;

public class TankVisuals : MonoBehaviour
{
    private MeshRenderer[] meshes;
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

    public void ShowTankDestroyed()
    {
        Debug.LogWarning("Show Tank Destroyed is NOT implemented yet...");
    }

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

    void Awake()
    {
        RefreshList();
        // ShowTankNormal(); // dont change UI on awake - need to show respawn UI which will show normal later
    }

    private void Start()
    {
        RefreshList();
    }

    public void RefreshList()
    {
        meshes = GetComponentsInChildren<MeshRenderer>();
    }
}
