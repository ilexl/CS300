using System;
using UnityEngine;
using UnityEngine.Serialization;

public class ProjectileSpawner : MaterialObject
{
    public Projectile projectilePrefab;

    public float projectileDiameterMm;

    public float projectileLengthMm;
    public float projectileVelocityMS;

    private float _diameterM;
    private float _lengthM;
    private float _volume;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Convert diameter and length from mm to meters
        _diameterM = projectileDiameterMm / 1000f;  // Convert mm to m
        _lengthM = projectileLengthMm / 1000f;      // Convert mm to m
        

        // Calculate the volume of the cylinder (projectile)
        _volume = Mathf.PI * Mathf.Pow(_diameterM / 2f, 2) * _lengthM;
        

        if (Material != null)
        {
            FireProjectile();
        }
        else
        {
            Debug.LogError("Material not found in the database!");
        }
    }

    void FireProjectile()
    {
        var projectileInstance = Instantiate(projectilePrefab, transform);
        projectileInstance.SetProjectileProperties(projectileVelocityMS, _diameterM, _lengthM, MaterialType);
    }
}
