using System;
using System.Collections;
using System.Collections.Generic;
using Ballistics.Database;
using UnityEngine;
using Random = System.Random;
using Vector3 = UnityEngine.Vector3;

namespace Ballistics
{
    /// <summary>
    /// A pool object for storing instances of projectiles, to be stored and reused.
    /// </summary>
    public class ProjectilePool
    {
        private static ProjectilePool _instance;
        public static ProjectilePool I => _instance ??= new ProjectilePool();

        private GameObject prefab;
        private Queue<GameObject> pool = new Queue<GameObject>();
        private int maxSize = 100000;

        private ProjectilePool()
        {
            // Fetch the prefab when the pool is accessed for the first time
            prefab = Resources.Load<GameObject>("Weaponry/Projectile");
            if (prefab == null)
            {
                throw new Exception("ProjectilePrefab not found in Resources!");
            }
        }
        /// <summary>
        /// Gets a projectile from the pool. Will create a new one if pool empty.
        /// </summary>
        /// <returns>Projectile GameObject</returns>
        public GameObject Get()
        {
            if (pool.Count > 0)
            {
                var obj = pool.Dequeue();
                obj.SetActive(true);
                
                return obj;
            }
            return GameObject.Instantiate(prefab);
        }
        /// <summary>
        /// Returns a projectile to the pool.
        /// </summary>
        /// <param name="obj">Projectile GameObject to return.</param>
        public void ReturnToPool(GameObject obj)
        {
            if (pool.Count < maxSize)
            {
                obj.SetActive(false);
                pool.Enqueue(obj);
            }
            else
            {
                GameObject.Destroy(obj);
            }
            
        }
    }
    /// <summary>
    /// A class used to store instances of projectile trail objects, to be stored and reused.
    /// </summary>
    public class ProjectileTrailDisplayPool
    {
        private static ProjectileTrailDisplayPool _instance;
        public static ProjectileTrailDisplayPool I => _instance ??= new ProjectileTrailDisplayPool();

        private GameObject prefab;
        private Queue<GameObject> pool = new Queue<GameObject>();

        private ProjectileTrailDisplayPool()
        {
            // Fetch the prefab when the pool is accessed for the first time
            prefab = Resources.Load<GameObject>("Weaponry/ProjectileTrail");
            if (prefab == null)
                throw new Exception("ProjectileTrail prefab not found in Resources!");
        }
        /// <summary>
        /// Gets a trail object from the pool. Will create a new object if pool empty.
        /// </summary>
        /// <returns>ProjectileTrail GameObject.</returns>
        public GameObject Get()
        {
            if (pool.Count > 0)
            {
                var obj = pool.Dequeue();
                obj.SetActive(true);
                return obj;
            }
            return GameObject.Instantiate(prefab);
        }

        /// <summary>
        /// Returns a ProjectileTrail to the pool.
        /// </summary>
        /// <param name="obj">ProjectileTrail GameObject to return.</param>
        public void ReturnToPool(GameObject obj)
        {
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    /// <summary>
    /// Manages timed returns of pooled objects.
    /// </summary>
    public static class CoroutineHelper
    {
        private class CoroutineRunner : MonoBehaviour { }

        private static CoroutineRunner _runner;
        private static bool _exists;
        
        private static void EnsureRunnerExists()
        {
            if (!_exists)
            {
                _exists = true;
                var go = new GameObject("CoroutineHelperRunner");
                GameObject.DontDestroyOnLoad(go); // Optional: keep alive across scenes
                _runner = go.AddComponent<CoroutineRunner>();
            }
        }

        public static Coroutine Run(IEnumerator coroutine)
        {
            EnsureRunnerExists();
            return _runner.StartCoroutine(coroutine);
        }
    }
    /// <summary>
    /// Main Projectile class. Contains functionality for velocity, diameter, length, and penetration. Capable of interacting with DamageableTankModules and their derrived classes.
    /// </summary>
    public class Projectile : MaterialObject
    {
        private float _diameter;
        private float _length;
        private static int _layerMask;
        private float _hpPool;
    
        private Vector3 _previousPos;
        [SerializeField]
        private Rigidbody rb;
        private Random rng;

        private int _framesAlive;
        private const int MaxFramesAlive = 60;
        private const float debugDrawDuration = 0.1f;
    
        public static List<Projectile> Projectiles = new List<Projectile>();

        public int penetrationCount = 0;

        /// <summary>
        /// Keeps various types of projectiles. For now, this is just a basic projectile and a spall. Functionally they are mostly identical save for some ricochet threshold differences.
        /// In the future, this would include rounds like HE, HESH, HEAT, etc.
        /// </summary>
        public enum ProjectileType
        {
            Bullet,
            Spall
        }

        private ProjectileType _type;     
    
        
        
        
        /// <summary>
        /// Creates a projectile at a position and a direction with a seed using an existing projectile definition.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="direction"></param>
        /// <param name="seed"></param>
        /// <param name="projectileDefinition"></param>
        /// <returns></returns>
        public static GameObject Create(Vector3 pos, Vector3 direction, long seed, ProjectileDefinition projectileDefinition)
        {
            var projectile = Create(pos, direction * projectileDefinition.VelocityMs, seed, projectileDefinition.DiameterMm / 1000f, projectileDefinition.LengthMm / 1000f, projectileDefinition.MaterialKey, ProjectileType.Bullet);
            return projectile;
        }
        /// <summary>
        /// Creates a projectile at a position with a velocity (direction and magnitude) with a seed, diameter, length, material key, and type
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="velocity"></param>
        /// <param name="seed"></param>
        /// <param name="diameterM"></param>
        /// <param name="lengthM"></param>
        /// <param name="mKey"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static GameObject Create(Vector3 pos, Vector3 velocity, long seed, float diameterM, float lengthM, MaterialKey mKey, ProjectileType type)
        {
            
            var projectile = Create(pos, velocity, new Random((int)seed), diameterM, lengthM, mKey, type);
            return projectile;
        }


        public static ProjectilePool projectilePool;
        /// <summary>
        /// Creates a projectile at a position with a velocity (direction and magnitude) with an rng object, diameter, length, material key, and type
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="velocity"></param>
        /// <param name="rng"></param>
        /// <param name="diameterM"></param>
        /// <param name="lengthM"></param>
        /// <param name="mKey"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static GameObject Create(Vector3 pos, Vector3 velocity, Random rng, float diameterM, float lengthM, MaterialKey mKey, ProjectileType type)
        {
            if (Projectiles.Count >= 100000) // Sane limit?
            {
                throw new Exception("Too many projectiles!");
            }

            // Get projectile GameObject from the pool
            var projectile = ProjectilePool.I.Get();

            // Get the Projectile component (already attached in prefab)
            var projectileInstance = projectile.GetComponent<Projectile>();

            // Rigidbody should be already attached in prefab, so no need to add
            // Just reset or update properties:
            projectileInstance.SetProjectileProperties(pos, velocity, diameterM, lengthM, mKey, type);
            projectileInstance.rng = rng;

            return projectile;
        }




        static Projectile()
        {
            _layerMask = LayerMask.GetMask("Damage System");
        }

        public void Start()
        {
            Projectiles.Add(this);
           
        }
        /// <summary>
        /// Sets the projectile properties of a new projectile, or one just taken from the pool.
        /// </summary>
        /// <param name="pos">The new position.</param>
        /// <param name="velocity">The new velocity.</param>
        /// <param name="diameterM">The new diameter.</param>
        /// <param name="lengthM">The new length.</param>
        /// <param name="mKey">The new material key.</param>
        /// <param name="type">The new type.</param>
        public void SetProjectileProperties(Vector3 pos, Vector3 velocity, float diameterM, float lengthM, MaterialKey mKey, ProjectileType type)
        {
            _framesAlive = 0;
            _type = type;
            transform.position = pos;
            _previousPos = pos;
        
            float velocityMS = velocity.magnitude;
        
            _diameter = diameterM;
            _length = lengthM;
            MaterialType = mKey;

        
            float density = Material.Density;  // Density in kg/m³

            float surfaceArea = Mathf.PI * Mathf.Pow(diameterM / 2f, 2);
            float volume = surfaceArea * lengthM;

            float mass = volume * density;
            rb.mass = mass;

            rb.linearVelocity = velocity;

            // DeMarre formula constants
        
            float k = Material.Hardness + Material.GetMaterialToughnessCoefficient(36, 0.2f);
            float n = 1.4f;

            float mOverD2 = mass / (diameterM * diameterM);
            _hpPool = k * Mathf.Sqrt(mOverD2) * Mathf.Pow(velocityMS, n);

            //Debug.Log("HP pool (penetration power) is " + _hpPool);

            float maxPenetration = GetMaximumPenetrationAgainstMaterial(MaterialKey.RolledHomogenousSteel);
            if (type == ProjectileType.Bullet)
            {
                Debug.Log("Maximum penetration is " + maxPenetration * 1000 + "mm" );
            }
        }
    
        public void FixedUpdate()
        {
            if (++_framesAlive > MaxFramesAlive)
            {
                Destroy();
            }
        
        

            int attempt = 0;

            float time = 1;
            // Perform raycasts until satisfaction condition met
            while (true)
            {
            
            
                if (++attempt >= 10) // Safety escape condition to avoid infinite loops
                {
                    throw new Exception("Projectile failed to penetrate plate");
                
                }
                // Setup for the initial raycast
                Vector3 to = transform.position - _previousPos;
                float mag = to.magnitude;
                Vector3 dir = to / mag;
                transform.position = _previousPos + to * time;
                
                // Perform the initial raycast and find the first object that is intersecting
                var didHit = Physics.Linecast(_previousPos, transform.position, out var hit, _layerMask);

                
                // And we remove the relative time from this frame to prevent overextending by going off multiple surfaces
                var hitTime = (hit.point - _previousPos).magnitude / mag;
                time -= hitTime;
                
                if (!didHit) // We didn't hit anything in this raycast attempt, so we break out, satisfied, and draw a line from our previous event pos to the new pos.
                {
                    DrawProjectileLine(_previousPos, transform.position);

                    break;
                }
                // We did hit something, so now we try to do further stuff with it, and draw a line to the initial hit point.
                DrawProjectileLine(_previousPos, hit.point);
                var impactType = PenetratePlate(hit, dir, ref time);
            
                // And then if we don't impact (degenerate case) or if we don't penetrate, we stop the loop entirely
                if (impactType is ImpactType.NoImpact or ImpactType.NonPenetrate) {break;}
            }
        
        
            _previousPos = transform.position;
        }

   

        /// <summary>
        /// Switch for impact type. Used for return state as well as what API function to fire.
        /// </summary>
        private enum ImpactType
        {
            Penetrate,
            NonPenetrate,
            Deflect,
            NoImpact
        }
        /// <summary>
        /// The primary penetration resolution loop. Outcomes can be a penetration, a non penetration, or a deflection, or a non hit in a degenerate case.
        /// </summary>
        /// <param name="entryHit">The start point of the event.</param>
        /// <param name="direction">The direction the projectile is travelling.</param>
        /// <param name="time">The time from 0-1 along the current frame.</param>
        /// <returns>The impact outcome.</returns>
        private ImpactType PenetratePlate(RaycastHit entryHit, Vector3 direction, ref float time)
        {
            // Set our default state for the collision resolution.
            ImpactType impactType = ImpactType.Penetrate;
        
            Vector3 entryPoint = entryHit.point;
            var panelGameObject = entryHit.collider.gameObject;
            // Here we get the collided object's panelComponent. If it cannot be found, IE the object is on the right layer but the script isn't attached, then this is a degenerate condition and must be handled.
            var tankComp = panelGameObject.GetComponent<DamageableTankModule>();
            
            // We get our baseline reflect angle in the case of a deflection.
            Vector3 potentialDeflectAngle = Vector3.Reflect(direction, entryHit.normal);
        
            
            // We get our cosTheta and tanTheta values between the direction of travel and plate normal.
            float cosTheta = Mathf.Abs(Vector3.Dot(direction.normalized, entryHit.normal.normalized));
            float tanTheta = Mathf.Sqrt(1 - cosTheta * cosTheta) / cosTheta;
            
        
            // We get the deflection factor - greater than 1 is a deflection, otherwise the projectile's travel path needs to be distorted along the plate's slope.
            var deflectionFactor = GetDeflectionFactor(tankComp, tanTheta);
            if (tankComp is FunctionalTankModule) deflectionFactor *= 0.0002f;
            //Debug.Log(deflectionFactor);
            if (deflectionFactor > 1f) // This is a deflection
            {
                impactType = ImpactType.Deflect;
                // We deflect the projectile entirely using the previously established potential deflection angle, and break out early since no more calculations are required.
                direction = ApplyDeflectionEffect(potentialDeflectAngle, entryPoint, cosTheta);
                _previousPos = entryPoint + direction * 0.00001f;
                //Debug.Log("Richochet...");
                return impactType;
            }
            // Else not explicitly required here but better consolidates the logical blocks
            else // This is projectile yaw (or possibly a non penetration)
            {
                // And here we difract the projectile along the plate's direction of slope. This is both realistic and implicitly models inherent plate slope protection benefits beyond LoS thickness.
                direction = ApplyDiffractionEffect(entryHit, direction, deflectionFactor, entryPoint);
            }

            // Because Unity lacks a built-in way to raycast "through" an object and get an exit point,
            // we have to iteratively step forward from the entry point and cast a ray back toward it.
            // This lets us detect when we've reached the opposite side of the object.

            RaycastHit secondHit = new RaycastHit();
            float backcastDist = 0.1f;

            // Step forward in increments, doubling each time, up to 10 meters.
            // The doubling helps cover distance quickly while still being precise near the entry.
            while (backcastDist <= 10f)
            {
                // Move forward along the projectile path by backcastDist
                var backCastPos = entryPoint + direction * backcastDist;

                // Raycast backwards from this position toward the entry point.
                // The ray length is proportional to backcastDist to avoid overshooting.
                bool didHit = RaycastUtility.RaycastToSpecificObject(
                    backCastPos,
                    -direction,
                    panelGameObject,
                    out secondHit,
                    backcastDist * 2f,
                    _layerMask
                );

                // If we hit something other than the entry point itself, we assume this is our exit point.
                if (didHit && secondHit.point != entryPoint)
                {
                    break;
                }

                // Increase the step size exponentially to cover larger distances efficiently.
                backcastDist *= 2f;
            }

            // If no exit point was found within 10 meters, treat it as a degenerate case and ignore it.
            if (backcastDist > 10f)
            {
                return ImpactType.NoImpact;
            }
            
            // And now we have our exitpoint....
            Vector3 exitPoint = secondHit.point  + direction * 0.00001f;
        
            // ... And our line of sight thickness.
            float thickness = Vector3.Dot(direction, exitPoint - entryPoint );
        
        
        
            // We get the raw protection value of the plate based on the material hardness, toughness, and LoS thickness.
            var protection = thickness * (tankComp.Material.Hardness + tankComp.Material.GetMaterialToughnessCoefficient(18, 0.75f)) * SpallableTankModule.ProtectionMultiplier;
        
            // We make the projectile lose energy in the case of a penetration.
            float lostEnergyRatio = (protection / _hpPool) / 1.3f;
            lostEnergyRatio = Mathf.Clamp01(lostEnergyRatio); // prevent negative or >1

            // We apply the energy loss to the projectile.
            Vector3 previousVelocity = rb.linearVelocity;
            rb.linearVelocity *= Mathf.Sqrt(1f - lostEnergyRatio);
            // And then we subtract from the HP pool.
            _hpPool -= protection;
            // If the HP pool is less then zero, that means the HP required to penetrate this plate has exceeded the maximum penetration of the projectile, leading to a definitive non penetration.
            if (_hpPool <= 0) impactType = ImpactType.NonPenetrate;
    
            // And we keep track of how many plates it's penetrated in general.
            penetrationCount++;
            
            switch (impactType) // Now that we know exactly what the projectile has done, we fire the API functions implemented by the objects we are shooting at.
            {
                case ImpactType.Penetrate:
                    _previousPos = exitPoint;
                    tankComp.PostPenetration(entryPoint, exitPoint, thickness, rb.linearVelocity, previousVelocity, _diameter, rng);
                    break;
                case ImpactType.NonPenetrate:
                    Destroy(); // Non pen is a special case, the object needs to be destroyed (returned to the pool).
                    tankComp.NonPenetration(entryPoint, exitPoint, thickness, rb.linearVelocity, previousVelocity, _diameter, rng);
                    break;
                case ImpactType.Deflect:
                    _previousPos = entryPoint;
                    tankComp.Deflection(entryPoint, exitPoint, thickness, rb.linearVelocity, previousVelocity, _diameter, rng);
                    break;
            }
    
            return impactType;
        }
    
        /// <summary>
        /// Our function for calculating and applying the diffraction effect.
        /// </summary>
        /// <param name="entryHit">The RaycastHit object representing the entry hit.</param>
        /// <param name="direction">The direction the projectile is travelling.</param>
        /// <param name="deflectionFactor">The product of the impact angle, projectile diameter, and speed that determine deflection susceptibility.</param>
        /// <param name="entryPoint">The actual point that we enter the object from.</param>
        /// <returns>The new direction.</returns>
        private Vector3 ApplyDiffractionEffect(RaycastHit entryHit, Vector3 direction, float deflectionFactor,
            Vector3 entryPoint)
        {
            // Our min and max deflection
            float deviationAngle = Mathf.Lerp(0f, 15f, deflectionFactor); // 0° at direct hit, 15° at grazing
            
            // We get the actual direction we need to be travelling after the penetration event...
            Vector3 exitDirection = UnityEngine.Quaternion.AngleAxis(deviationAngle, Vector3.Cross(direction, entryHit.normal)) * direction;
            
            // ... And we apply it.
            direction = exitDirection;
            rb.linearVelocity = direction * rb.linearVelocity.magnitude;
            
            transform.position = entryPoint + rb.linearVelocity / 60;
        
            return direction;
        }
        /// <summary>
        /// A helper function that manages applying the deflection to a projectile, and the energy loss induced by that deflection.
        /// </summary>
        /// <param name="deflectDirection">The direction we want the projectile to deflect to.</param>
        /// <param name="entryPoint">The initial point of contact with the object.</param>
        /// <param name="cosTheta">The theta cosine.</param>
        /// <returns>The deflection direction.</returns>
        private Vector3 ApplyDeflectionEffect(Vector3 deflectDirection, Vector3 entryPoint, float cosTheta)
        {
            rb.linearVelocity = deflectDirection * rb.linearVelocity.magnitude;
            transform.position = entryPoint + rb.linearVelocity / 60f;
        
            // Calculate energy loss ratio based on deflection severity
            float deflectEnergyLossRatio = 1f - Mathf.Exp(-cosTheta * 3f);
    
    
            // Apply velocity loss respecting kinetic energy scaling
            rb.linearVelocity *= Mathf.Sqrt(1f - deflectEnergyLossRatio);
    
            // Apply HP pool loss based on lost energy (HP loss ∝ energy lost)
            float lostEnergy = 0.5f * rb.mass * (Mathf.Pow(rb.linearVelocity.magnitude / Mathf.Sqrt(1f - deflectEnergyLossRatio), 2) - Mathf.Pow(rb.linearVelocity.magnitude, 2));
            _hpPool -= lostEnergy;
    
            // Clamp HP pool to non-negative
            _hpPool = Mathf.Max(0f, _hpPool);
            return deflectDirection;
        }
        /// <summary>
        /// Gets the potential to deflect from a given impact angle.
        /// </summary>
        /// <param name="panel">The panel game object.</param>
        /// <param name="tanTheta">The theta tangent.</param>
        /// <returns>The deflection factor.</returns>
        private float GetDeflectionFactor(DamageableTankModule panel, float tanTheta)
        {
            // GOAL: APFSDS-like projectiles should deflect at ~11 degrees. WWII-esque projectiles should deflect around 60 degrees.
            float hardnessRatio = panel.Material.Hardness / Material.Hardness;
            float impactResistanceRatio = panel.Material.ImpactResistance / Material.ImpactResistance;
            float velocityFactor = 1f / rb.linearVelocity.sqrMagnitude;
            float surfaceArea = Mathf.PI * Mathf.Pow(_diameter / 2f, 2);
            float massSurfaceAreaRatio = surfaceArea / rb.mass;
    
            float deflectFactorModifier = hardnessRatio * impactResistanceRatio * velocityFactor * massSurfaceAreaRatio * 3000000000; // Don't we all love magic numbers?? (I really don't know what I'm doing but it looks good on screen!!)
        
            float deflectionFactor = deflectFactorModifier * tanTheta;
            return deflectionFactor;
        }

        /// <summary>
        /// Main Destroy function. Doesn't actually destroy, rather returns to the pool.
        /// </summary>
        public void Destroy()
        {
            Projectiles.Remove(this);
            ProjectilePool.I.ReturnToPool(gameObject);
            Debug.Log("Returning projectile to pool");
        }
    
    
    
        /// <summary>
        /// Helper function to get as a value the maximum penetration that this projectile could do against a given material.
        /// </summary>
        /// <param name="mKey"></param>
        /// <returns></returns>
        private float GetMaximumPenetrationAgainstMaterial(MaterialKey mKey)
        {
            var material = MaterialDatabase.GetMaterial(mKey);
        
            return _hpPool / ((material.Hardness + material.GetMaterialToughnessCoefficient(18, 0.75f)) * SpallableTankModule.ProtectionMultiplier);
        }
    
        /// <summary>
        /// Draws a line between point a and point b using the projectiles diameter.
        /// </summary>
        /// <param name="posA">posA</param>
        /// <param name="posB">posB</param>
        private void DrawProjectileLine(Vector3 posA, Vector3 posB)
        {
            
            
            Vector3 shapeCenter = (posA + posB) / 2;
            GameObject line = ProjectileTrailDisplayPool.I.Get();

            var lineRenderer = line.GetComponent<LineRenderer>();
            
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, posA);
            lineRenderer.SetPosition(1, posB);

            lineRenderer.startWidth = _diameter;
            lineRenderer.endWidth = _diameter;
            
            CoroutineHelper.Run(ReturnAfterDelay(line, 0.1f));

            IEnumerator ReturnAfterDelay(GameObject obj, float delay)
            {
                yield return new WaitForSeconds(delay);
                ProjectileTrailDisplayPool.I.ReturnToPool(obj);
            }
        }
        
    }
}