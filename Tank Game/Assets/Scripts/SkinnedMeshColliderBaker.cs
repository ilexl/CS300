using UnityEngine;

public class SkinnedMeshColliderBaker : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        var bakedMesh = new Mesh();
        skinnedMeshRenderer.BakeMesh(bakedMesh);

        var meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = bakedMesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
