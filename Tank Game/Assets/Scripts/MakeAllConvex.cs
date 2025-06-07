using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MakeAllConvex : MonoBehaviour
{
    public void Apply()
    {
        TraverseAndFix(transform);
    }

    private void TraverseAndFix(Transform parent)
    {
        // If this object has a MeshCollider, try to set it to convex
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