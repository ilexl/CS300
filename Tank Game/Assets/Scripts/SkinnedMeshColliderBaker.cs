using UnityEngine;

/// <summary>
/// Automatically bakes a SkinnedMeshRenderer's mesh into a MeshCollider at runtime.
/// Useful for enabling accurate physics or hit detection on animated characters.
/// </summary>
public class SkinnedMeshColliderBaker : MonoBehaviour
{
    /// <summary>
    /// Bakes the current state of the SkinnedMeshRenderer into a static MeshCollider at start.
    /// </summary>
    void Start()
    {
        var skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        var bakedMesh = new Mesh();
        skinnedMeshRenderer.BakeMesh(bakedMesh);

        // Add a MeshCollider and assign the baked mesh
        var meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = bakedMesh;
    }
}
