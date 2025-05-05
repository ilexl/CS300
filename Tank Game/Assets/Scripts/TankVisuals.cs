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
        HUDUI.current.HideSniperMode();
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
        HUDUI.current.ShowSniperMode();
    }

    void Awake()
    {
        RefreshList();
        ShowTankNormal(); // show normal on awake
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
