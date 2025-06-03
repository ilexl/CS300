using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;
using System.IO;

public class ProBuilderPrefabCleaner : EditorWindow
{
    [MenuItem("Tools/ProBuilder/Convert ProBuilder Prefab To Cleaned Version")]
    static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ProBuilderPrefabCleaner));
    }

    GameObject prefabToClean;

    void OnGUI()
    {
        GUILayout.Label("Convert ProBuilder Prefab", EditorStyles.boldLabel);
        prefabToClean = (GameObject)EditorGUILayout.ObjectField("Prefab", prefabToClean, typeof(GameObject), false);

        if (prefabToClean != null)
        {
            if (GUILayout.Button("Convert and Save New Prefab"))
            {
                ConvertPrefab(prefabToClean);
            }
        }
    }

    void ConvertPrefab(GameObject prefab)
{
    string prefabPath = AssetDatabase.GetAssetPath(prefab);
    string prefabDir = Path.GetDirectoryName(prefabPath);
    string prefabName = Path.GetFileNameWithoutExtension(prefabPath);
    string meshDir = prefabDir + "/ConvertedMeshes/";

    if (!Directory.Exists(meshDir))
        Directory.CreateDirectory(meshDir);

    GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

    GameObject root = new GameObject(prefab.name + "_Cleaned");

    // Recursively copy the hierarchy
    CopyHierarchy(instance.transform, root.transform, meshDir, prefabName);

    // Save new prefab
    string cleanedPrefabPath = prefabDir + "/" + prefabName + "_cleaned.prefab";
    PrefabUtility.SaveAsPrefabAsset(root, cleanedPrefabPath);

    // Cleanup
    DestroyImmediate(instance);
    DestroyImmediate(root);

    Debug.Log($"Converted and saved cleaned prefab: {cleanedPrefabPath}");
}

void CopyHierarchy(Transform source, Transform targetParent, string meshDir, string prefabName)
{
    GameObject newObj = new GameObject(source.name);
    newObj.transform.SetParent(targetParent);
    newObj.transform.localPosition = source.localPosition;
    newObj.transform.localRotation = source.localRotation;
    newObj.transform.localScale = source.localScale;
    newObj.layer = source.gameObject.layer;
    newObj.tag = source.gameObject.tag;
    newObj.SetActive(source.gameObject.activeSelf);

    // Check for ProBuilderMesh
    var pbMesh = source.GetComponent<ProBuilderMesh>();
    if (pbMesh && pbMesh is ProBuilderMesh)
    {
        pbMesh.ToMesh();
        pbMesh.Refresh();

        var mf = pbMesh.GetComponent<MeshFilter>();
        Mesh finalMesh = Object.Instantiate(mf.sharedMesh);

        string safePathName = GetFullPath(source).Replace("/", "_").Replace(" ", "_");
        string meshAssetPath = meshDir + prefabName + "_" + safePathName + ".asset";

        AssetDatabase.CreateAsset(finalMesh, meshAssetPath);
        AssetDatabase.SaveAssets();

        var newMF = newObj.AddComponent<MeshFilter>();
        newMF.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshAssetPath);

        var originalMR = source.GetComponent<MeshRenderer>();
        if (originalMR)
        {
            var mr = newObj.AddComponent<MeshRenderer>();
            mr.sharedMaterials = originalMR.sharedMaterials;
        }
    }
    // Copy other common components — colliders, lights, scripts, etc.
    foreach (var comp in source.GetComponents<Component>())
    {
        if (comp is Transform or ProBuilderMesh || comp.GetType().Name == "ProBuilderShape")
            continue; // already handled or skipped

        UnityEditorInternal.ComponentUtility.CopyComponent(comp);
        UnityEditorInternal.ComponentUtility.PasteComponentAsNew(newObj);
    }
    // Recursively copy children
    foreach (Transform child in source)
    {
        CopyHierarchy(child, newObj.transform, meshDir, prefabName);
    }
}

    
    string GetFullPath(Transform obj)
    {
        string path = obj.name;
        while (obj.parent != null)
        {
            path = obj.parent.name + "/" + path;
            obj = obj.parent;
        }
        return path;
    }
}
