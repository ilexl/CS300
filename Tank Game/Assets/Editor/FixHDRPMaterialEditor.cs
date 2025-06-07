using UnityEngine;
using UnityEditor;

public class FixHDRPMaterialEditor
{
    private const string MaterialPath = "Assets/DefaultHDRPMaterial.mat";

    [MenuItem("Tools/Fix HDRP Default Material")]
    public static void FixHDRPMaterial()
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);

        if (mat == null)
        {
            mat = new Material(Shader.Find("HDRP/Lit"));
            AssetDatabase.CreateAsset(mat, MaterialPath);
            Debug.Log("Created new HDRP Default Material at: " + MaterialPath);
        }

        if (mat.shader.name != "HDRP/Lit")
        {
            mat.shader = Shader.Find("HDRP/Lit");
            Debug.Log("Changed shader to HDRP/Lit.");
        }

        // Apply correct settings
        mat.SetColor("_BaseColor", new Color(0.5f, 0.5f, 0.5f, 1f)); // Light grey
        mat.SetFloat("_Metallic", 0f);
        mat.SetFloat("_Smoothness", 0.5f);
        mat.DisableKeyword("_EMISSION");
        mat.SetColor("_EmissiveColor", Color.black);
        mat.SetFloat("_EmissiveIntensity", 0f);

        EditorUtility.SetDirty(mat);
        AssetDatabase.SaveAssets();
        Debug.Log("HDRP Default Material fixed and saved.");
    }
}
