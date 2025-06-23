using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Utility to recursively set all MeshColliders in this GameObject hierarchy to convex.
/// Useful for ensuring physics compatibility, especially for mesh colliders that need to be convex.
/// Includes an Editor button to run this fix from the Inspector.
/// </summary>
public class MakeAllConvex : MonoBehaviour
{
    /// <summary>
    /// Starts the recursive process to find and fix all MeshColliders under this transform.
    /// </summary>
    public void Apply()
    {
        TraverseAndFix(transform);
    }

    /// <summary>
    /// Recursively traverses the transform hierarchy starting at 'parent' and sets any found MeshCollider to convex.
    /// Logs each change for user feedback.
    /// </summary>
    /// <param name="parent">Current transform to check and traverse.</param>
    private void TraverseAndFix(Transform parent)
    {
        // If this GameObject has a MeshCollider that is not convex, set it to convex.
        MeshCollider meshCollider = parent.GetComponent<MeshCollider>();
        if (meshCollider != null)
        {
            if (!meshCollider.convex)
            {
                meshCollider.convex = true;
                Debug.Log($"Set convex = true on {parent.name}", parent.gameObject);
            }
        }

        // Recurse through all children
        foreach (Transform child in parent)
        {
            TraverseAndFix(child);
        }
    }
}

/// <summary>
/// Custom inspector for MakeAllConvex.
/// Adds a "Fix" button to apply convex conversion to all MeshColliders in the hierarchy.
/// </summary>
#if UNITY_EDITOR
[CustomEditor(typeof(MakeAllConvex))]
public class EDITOR_MakeAllConvex : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Fix"))
        {
            MakeAllConvex m = (target as MakeAllConvex);
            m.Apply();
        }
    }
}
#endif