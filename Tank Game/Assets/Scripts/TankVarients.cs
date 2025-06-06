using System;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NewTank", menuName = "Tank/Create New Tank")]
[System.Serializable]
public class TankVarients : ScriptableObject
{
    public string tankName;
    public string Nation, Rank, Icon;

    [Header("Cannons")]
    public GameObject[] cannonModels;
    [Tooltip("in meters")] public float[] cannonDiameters;
    [Tooltip("in meters")] public float[] cannonLengths;
    public Vector3[] cannonPivotPoints;
    public Vector3[] cannonPositions;
    public Vector3[] cannonRotations;
    public int[] cannonAttachedToTurretIndexs;

    [Header("Turrets")]
    public GameObject[] turretModels;
    [Tooltip("in meters")] public float[] turretWidths;
    [Tooltip("in meters")] public float[] turretLengths;
    [Tooltip("in meters")] public float[] turretHeights;
    public Vector3[] turretPivotPoints;
    public Vector3[] turretPositions;
    public Vector3[] turretRotations;

    [Header("Hull")]
    public GameObject hullModel;
    [Tooltip("in meters")] public float hullWidth;
    [Tooltip("in meters")] public float hullLength;
    [Tooltip("in meters")] public float hullHeight;
    public Vector3 hullRotation;
    public Vector3 hullPosition;

    [Header("Parameters")]
    public Vector3 modelScale;
    [Tooltip("in m/s")] public float[] turretRotationSpeed;
    [Tooltip("in m/s")] public float[] gunAimSpeed;
    [Tooltip("in m/s")] public float tankMoveSpeed;
    public Vector3 cameraPosSniper;





    /// <summary>
    /// Loads a TankVarients ScriptableObject from Resources/Tanks by name.
    /// </summary>
    /// <param name="tankName">The name of the tank asset (must match filename)</param>
    /// <returns>TankVarients if found, otherwise null</returns>
    public static TankVarients GetFromString(string tankName)
    {
        if (string.IsNullOrWhiteSpace(tankName))
        {
            Debug.LogWarning("Tank name is null or empty.");
            return null;
        }

        TankVarients tank = Resources.Load<TankVarients>($"Tanks/{tankName}");

        if (tank == null)
        {
            Debug.LogWarning($"TankVarient '{tankName}' not found in Resources/Tanks/");
        }

        return tank;
    }

    public static Texture GetTextureFromString(string imageName)
    {
        Texture raw = Resources.Load<Texture>($"Tanks/Icons/{imageName}");
        if ( raw == null)
        {
            raw = Resources.Load<Texture>($"Tanks/Icons/Placeholder");
        }
        return raw; 
    }
    

}
