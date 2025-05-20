using System;
using Ballistics;
using UnityEngine;
using Random = UnityEngine.Random;


public class SpallableObject : MaterialObject
{
    public static readonly float ProtectionMultiplier = 2650000; // Dumb global variable to get armour value in the same ballpark as projectile damage pools
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    float referenceVelocity = 2000f;
    float k = 20f; // Cone scalar
    float a = 0.5f; // How cone size relates to armour matching
    float b = 0.75f; // How cone size scales by velocity

    public void PostPenetration(
    Vector3 entryPoint,
    Vector3 exitPoint,
    float thickness,
    Vector3 projectileVelocity,
    float projectileDiameter)
{
    // Parameters for spall generation
    float baseFragmentCount = thickness * projectileDiameter * 10000f; // tune multiplier for density
    int fragmentCount = Math.Clamp((int)baseFragmentCount, 0, 500);  // clamp for min/max fragments

    float baseFragmentSize = projectileDiameter * 0.1f;             // base fragment size relative to projectile
    float sizeStdDev = baseFragmentSize * 0.3f;                     // standard deviation of fragment size

    float baseSpallSpeedFactor = 0.3f;                              // fraction of projectile velocity for spall speed
    float spallSpeedStdDevFactor = 0.1f;                            // randomness factor for spall speed

    float radius = projectileDiameter * 0.5f;
    float ratio = radius / thickness;
    float velocityFactor = referenceVelocity / Mathf.Max(projectileVelocity.magnitude, 1f);

    float spallConeAngleDeg = k * Mathf.Pow(ratio, a) * Mathf.Pow(velocityFactor, b);
    //spallConeAngleDeg = Mathf.Clamp(spallConeAngleDeg, 5f, 80f);  // optional sensible bounds

    // Convert to radians for use
    float spallConeAngleRad = spallConeAngleDeg * Mathf.Deg2Rad;
    
    System.Random rng = new System.Random();

    Vector3 penetrationDirection = Vector3.Normalize(exitPoint - entryPoint);

    for (int i = 0; i < fragmentCount; i++)
    {
        // Fragment size with normal distribution approx using Box-Muller transform
        float size = MathF.Max(0.0001f, (float)NormalRandom(rng, baseFragmentSize, sizeStdDev));

        // Fragment velocity magnitude with some randomness
        float speed = MathF.Max(0f, baseSpallSpeedFactor * projectileVelocity.magnitude +
                               (float)NormalRandom(rng, 0, spallSpeedStdDevFactor * projectileVelocity.magnitude));

        // Random direction within the spall cone
        Vector3 direction = RandomDirectionInCone(rng, penetrationDirection, spallConeAngleRad);

        Vector3 fragmentVelocity = direction * speed;

        // Spawn your spall fragment here with:
        // position = exitPoint
        // velocity = fragmentVelocity
        // size = size

        SpawnSpallFragment(exitPoint, fragmentVelocity, size);
    }
}

// Helper: Generate normally distributed random numbers using Box-Muller transform
private double NormalRandom(System.Random rng, double mean, double stdDev)
{
    double u1 = 1.0 - rng.NextDouble(); // uniform(0,1] random doubles
    double u2 = 1.0 - rng.NextDouble();
    double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                           Math.Sin(2.0 * Math.PI * u2); // random normal(0,1)
    return mean + stdDev * randStdNormal;
}

// Helper: Generate a random direction vector inside a cone
    private Vector3 RandomDirectionInCone(System.Random rng, Vector3 coneDirection, float coneAngleRad)
    {
        // Random azimuth 0 to 2π
        double phi = rng.NextDouble() * 2.0 * Math.PI;

        // Biased random elevation (0 to coneAngleRad)
        // Square of NextDouble() biases towards zero (more central clustering)
        double theta = Math.Pow(rng.NextDouble(), 2.0) * coneAngleRad;

        // Convert spherical to Cartesian (assuming cone along +Z)
        double sinTheta = Math.Sin(theta);
        Vector3 localDir = new Vector3(
            (float)(sinTheta * Math.Cos(phi)),
            (float)(sinTheta * Math.Sin(phi)),
            (float)Math.Cos(theta)
        );

        // Rotate localDir to align with coneDirection
        return RotateVectorToDirection(localDir, coneDirection);
    }


// Helper: Rotate vector from local +Z to target direction
private Vector3 RotateVectorToDirection(Vector3 vec, Vector3 targetDir)
{
    Vector3 zAxis = new Vector3(0, 0, 1);
    Vector3 axis = Vector3.Cross(zAxis, targetDir);
    float angle = MathF.Acos(Vector3.Dot(zAxis, targetDir));

    if (axis.sqrMagnitude < 1e-6f) // Vectors are parallel
    {
        return targetDir.z < 0 ? -vec : vec; // flip if opposite direction
    }

    axis = Vector3.Normalize(axis);

    return RotateAroundAxis(vec, axis, angle);
}

// Helper: Rotate vector 'v' around axis 'axis' by angle radians (Rodrigues' rotation formula)
private Vector3 RotateAroundAxis(Vector3 v, Vector3 axis, float angle)
{
    float cos = MathF.Cos(angle);
    float sin = MathF.Sin(angle);

    return v * cos + Vector3.Cross(axis, v) * sin + axis * (Vector3.Dot(axis, v) * (1 - cos));
}

    private void SpawnSpallFragment(Vector3 position, Vector3 velocity, float size)
    {
        // Create a new projectile using the current plate's material
        Projectile.Create(position, velocity, size, size, MaterialType, Projectile.ProjectileType.spall);
    }
}
