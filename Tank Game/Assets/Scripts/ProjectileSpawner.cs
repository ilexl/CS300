using System;
using UnityEngine;
using UnityEngine.Serialization;

public class ProjectileSpawner : MonoBehaviour
{
    private MaterialKey _materialType;
    public MaterialKey MaterialType
    {
        get => _materialType;
        set
        {
            _materialType = value;
            _material = MaterialDatabase.GetMaterial(_materialType);
        }
    }
    private Material _material;

    [FormerlySerializedAs("projectileDiameterMM")] public float projectileDiameterMm;

    [FormerlySerializedAs("projectileLengthMM")] public float projectileLengthMm;
    public float projectileVelocityMS;

    private float _volume;
    private float _mass;
    private float _kineticEnergy;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Convert diameter and length from mm to meters
        float diameterM = projectileDiameterMm / 1000f;  // Convert mm to m
        float lengthM = projectileLengthMm / 1000f;      // Convert mm to m
        

        // Calculate the volume of the cylinder (projectile)
        _volume = Mathf.PI * Mathf.Pow(diameterM / 2f, 2) * lengthM;
        

        if (_material != null)
        {
            float density = _material.Density;  // Density in kg/m³

            // Calculate mass
            _mass = _volume * density;
            _kineticEnergy = (float)(0.5 * _mass * Math.Pow(projectileVelocityMS, 2));
            Debug.Log($"Projectile Mass: {_mass} kg");
            Debug.Log($"Projectile KE: {_kineticEnergy} joules");
        }
        else
        {
            Debug.LogError("Material not found in the database!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
