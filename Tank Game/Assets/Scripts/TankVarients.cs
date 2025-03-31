using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTank", menuName = "Tank/Create New Tank")]
[System.Serializable]
public class TankVarients : ScriptableObject
{
    public string tankName;

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
    [Tooltip("in m/s")] float[] turretRotationSpeed;
    [Tooltip("in m/s")] float[] gunAimSpeed;
    [Tooltip("in m/s")] float tankMoveSpeed;
}
