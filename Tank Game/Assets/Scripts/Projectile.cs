using System;
using UnityEngine;

public class Projectile : MaterialObject
{
    private float _diameter;
    private float _length;
    public Rigidbody rb;
    

    public void SetProjectileProperties(float velocityMS, float diameterM, float lengthM, MaterialKey mKey)
    {
        _diameter = diameterM;
        _length = lengthM;
        MaterialType = mKey;
        
        
        float density = Material.Density;  // Density in kg/m³

        float volume = Mathf.PI * Mathf.Pow(diameterM / 2f, 2) * lengthM;
        // Calculate mass
        float mass = volume * density;
        float kineticEnergy = (float)(0.5 * mass * Math.Pow(velocityMS, 2));

        rb.mass = mass;
        rb.AddForce(transform.forward * kineticEnergy);
        // Deflection mechanics, volume, KE, etc can all be determined from these parameters
    }

    public void FixedUpdate()
    {
       Debug.Log(rb.linearVelocity.magnitude);
    }
}