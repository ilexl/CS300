using System;
using System.Collections;
using System.Collections.Generic;
using Ballistics.Database;
using UnityEngine;
using Random = System.Random;
using Vector3 = UnityEngine.Vector3;

namespace Ballistics
{
    public class ProjectilePool
    {
        private static ProjectilePool _instance;
        public static ProjectilePool I => _instance ??= new ProjectilePool();

        private GameObject prefab;
        private Queue<GameObject> pool = new Queue<GameObject>();
        private int maxSize = 100000;

        private ProjectilePool()
        {
            prefab = Resources.Load<GameObject>("Weaponry/Projectile");
            if (prefab == null)
            {
                throw new Exception("ProjectilePrefab not found in Resources!");
            }
        }

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
    
    public class ProjectileTrailDisplayPool
    {
        private static ProjectileTrailDisplayPool _instance;
        public static ProjectileTrailDisplayPool I => _instance ??= new ProjectileTrailDisplayPool();

        private GameObject prefab;
        private Queue<GameObject> pool = new Queue<GameObject>();

        private ProjectileTrailDisplayPool()
        {
            prefab = Resources.Load<GameObject>("Weaponry/ProjectileTrail");
            if (prefab == null)
                throw new Exception("ProjectileTrail prefab not found in Resources!");
        }

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

        public void ReturnToPool(GameObject obj)
        {
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

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
    
    public class Projectile : MaterialObject
    {
        private float _diameter;
        private float _length;
        private int _layerMask;
        private float _hpPool;
    
        private Vector3 _previousPos;
        [SerializeField]
        private Rigidbody rb;
        private System.Random rng;

        private int _framesAlive;
        private const int MaxFramesAlive = 60;
        private const float debugDrawDuration = 0.1f;
    
        public static List<Projectile> Projectiles = new List<Projectile>();

        public int penetrationCount = 0;

    
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

        

        


        public void Start()
        {
            Projectiles.Add(this);
            _layerMask = LayerMask.GetMask("Damage System");   // Why can't I make this static, unity?? Are you trying to make optimization impossible?
        }
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
            while (true)
            {
            
            
                if (++attempt >= 10)
                {
                    throw new Exception("Projectile failed to penetrate plate");
                
                }
                Vector3 to = transform.position - _previousPos;
                float mag = to.magnitude;
                Vector3 dir = to / mag;
                transform.position = _previousPos + to * time;
                var didHit = Physics.Linecast(_previousPos, transform.position, out var hit, _layerMask);

                var hitTime = (hit.point - _previousPos).magnitude / mag;
                time -= hitTime;
                
                if (!didHit)
                {
                    DrawMesh(_previousPos, transform.position);

                    break;
                }
                DrawMesh(_previousPos, hit.point);
                var impactType = PenetratePlate(hit, dir, ref time);
            
                // Basically we want to keep attempting to step the projectile forward until it has either not hit anything in this substep or has non penned
                if (impactType is ImpactType.NoImpact or ImpactType.NonPenetrate) {break;}



            }
        
        
            _previousPos = transform.position;
        }

   


        private enum ImpactType
        {
            Penetrate,
            NonPenetrate,
            Deflect,
            NoImpact
        }

        private ImpactType PenetratePlate(RaycastHit entryHit, Vector3 direction, ref float time)
        {
            ImpactType impactType = ImpactType.Penetrate;
        
            Vector3 entryPoint = entryHit.point;
            var panelGameObject = entryHit.collider.gameObject;
            // TODO: Generalize to DamageableComponent once implemented
            var tankComp = panelGameObject.GetComponent<DamageableTankModule>();
            
            Vector3 potentialDeflectAngle = Vector3.Reflect(direction, entryHit.normal);
        
            float cosTheta = Mathf.Abs(Vector3.Dot(direction.normalized, entryHit.normal.normalized));
            float tanTheta = Mathf.Sqrt(1 - cosTheta * cosTheta) / cosTheta;
        
        
            var deflectionFactor = GetDeflectionFactor(tankComp, tanTheta);
            if (tankComp is FunctionalTankModule) deflectionFactor *= 0.0002f;
            //Debug.Log(deflectionFactor);
            if (deflectionFactor > 1f) // This is a deflection
            {
                impactType = ImpactType.Deflect;
                direction = ApplyDeflectionEffect(potentialDeflectAngle, entryPoint, cosTheta);
                _previousPos = entryPoint + direction * 0.00001f;
                //Debug.Log("Richochet...");
                return impactType;
            }
            else // This is projectile yaw (or possibly a non penetration)
            {
                // Calculate the diffraction from travelling through the plate
                direction = ApplyDiffractionEffect(entryHit, direction, deflectionFactor, entryPoint);
            }


            RaycastHit secondHit = new RaycastHit();
            float backcastDist = 0.1f;
            while (backcastDist <= 10f)
            {
                var backCastPos = entryPoint + direction * backcastDist;
                bool didHit = RaycastUtility.RaycastToSpecificObject(backCastPos, -direction, panelGameObject, out secondHit, backcastDist * 2f, _layerMask);
            

                if (didHit && secondHit.point != entryPoint) 
                {
                    break;
                }
                backcastDist *= 2f;
            }

            if (backcastDist > 10f)
            {
                //Debug.Log("Missed???");
                return ImpactType.NoImpact;
            }
            Vector3 exitPoint = secondHit.point  + direction * 0.00001f;
        
        
            float thickness = Vector3.Dot(direction, exitPoint - entryPoint );
        
        
        
        
            var protection = thickness * (tankComp.Material.Hardness + tankComp.Material.GetMaterialToughnessCoefficient(18, 0.75f)) * SpallableTankModule.ProtectionMultiplier;
            float distance = GetMaximumPenetrationAgainstMaterial(tankComp.MaterialType);
        
            float lostEnergyRatio = (protection / _hpPool) / 1.3f;
            lostEnergyRatio = Mathf.Clamp01(lostEnergyRatio); // prevent negative or >1

            Vector3 previousVelocity = rb.linearVelocity;
            rb.linearVelocity *= Mathf.Sqrt(1f - lostEnergyRatio);
            _hpPool -= protection;
            //Debug.Log(_hpPool);
            if (_hpPool <= 0) impactType = ImpactType.NonPenetrate;
    
            penetrationCount++;
    
            //Debug.Log(impactType);
            switch (impactType)
            {
                case ImpactType.Penetrate:
                    _previousPos = exitPoint;
                    tankComp.PostPenetration(entryPoint, exitPoint, thickness, rb.linearVelocity, previousVelocity, _diameter, rng);
                    break;
                case ImpactType.NonPenetrate:
                    Destroy();
                    tankComp.NonPenetration(entryPoint, exitPoint, thickness, rb.linearVelocity, previousVelocity, _diameter, rng);
                    break;
                case ImpactType.Deflect:
                    _previousPos = entryPoint;
                    tankComp.Deflection(entryPoint, exitPoint, thickness, rb.linearVelocity, previousVelocity, _diameter, rng);
                    break;
            }
    
            return impactType;
        }
    
    
        private Vector3 ApplyDiffractionEffect(RaycastHit entryHit, Vector3 direction, float deflectionFactor,
            Vector3 entryPoint)
        {
            float deviationAngle = Mathf.Lerp(0f, 15f, deflectionFactor); // 0° at direct hit, 15° at grazing
            Vector3 exitDirection = UnityEngine.Quaternion.AngleAxis(deviationAngle, Vector3.Cross(direction, entryHit.normal)) * direction;
            direction = exitDirection;
            rb.linearVelocity = direction * rb.linearVelocity.magnitude;
            
            transform.position = entryPoint + rb.linearVelocity / 60;
        
            return direction;
        }

        private Vector3 ApplyDeflectionEffect(Vector3 direction, Vector3 entryPoint, float cosTheta)
        {
            rb.linearVelocity = direction * rb.linearVelocity.magnitude;
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
            return direction;
        }

        private float GetDeflectionFactor(DamageableTankModule panel, float tanTheta)
        {
            // GOAL: APFSDS-like projectiles should deflect at ~11 degrees. WWII-esque projectiles should deflect around 60 degrees.
            float hardnessRatio = panel.Material.Hardness / Material.Hardness;
            float impactResistanceRatio = panel.Material.ImpactResistance / Material.ImpactResistance;
            float velocityFactor = 1f / rb.linearVelocity.sqrMagnitude;
            float surfaceArea = Mathf.PI * Mathf.Pow(_diameter / 2f, 2);
            float massSurfaceAreaRatio = surfaceArea / rb.mass;
    
            float deflectFactorModifier = hardnessRatio * impactResistanceRatio * velocityFactor * massSurfaceAreaRatio * 3000000000; // Don't we all love magic numbers?? (I really don't know what I'm doing but it looks good on screen!!)
            // 100000
        
            float deflectionFactor = deflectFactorModifier * tanTheta;
            return deflectionFactor;
        }


        public void Destroy()
        {
            Projectiles.Remove(this);
            ProjectilePool.I.ReturnToPool(gameObject);
            Debug.Log("Returning projectile to pool");
        }
    
    
    
    
        private float GetMaximumPenetrationAgainstMaterial(MaterialKey mKey)
        {
            var material = MaterialDatabase.GetMaterial(mKey);
        
            return _hpPool / ((material.Hardness + material.GetMaterialToughnessCoefficient(18, 0.75f)) * SpallableTankModule.ProtectionMultiplier);
        }
    
        private void DrawMesh(Vector3 posA, Vector3 posB)
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
    
    
    
    
    
    
    
    
    
    
    
        // An attempt at CSG-based plate damage
        // private void DamagePlate(Collider[] colliders )    
        // {
        //     
        //     Vector3 shapeCenter = (_previousPos + transform.position) / 2;
        //     Vector3 pointA = _previousPos - shapeCenter;
        //     Vector3 pointB = transform.position - shapeCenter;
        //     
        //     Mesh cyl = ProjectileDamageMeshGenerator.Generate(pointA, pointB, _diameter / 2, 10);
        //     GameObject negative = new GameObject("CylinderDisplay");
        //     
        //     
        //     negative.AddComponent<MeshFilter>().mesh = cyl;
        //     negative.AddComponent<MeshRenderer>().material = unityMaterial;
        //     negative.transform.position = shapeCenter;
        //     
        //     // Note to self: Material on plate and negative MUST MATCH!! Don't care how, just need to be the same when doing CSG.
        //     for (int i = 0; i < colliders.Length; i++)
        //     {
        //         // Now do damage to the plate
        //         var plate = colliders[i].gameObject;
        //         
        //         var meshFilter = plate.GetComponent<MeshFilter>();
        //         plate.GetComponent<MeshRenderer>().material = unityMaterial;
        //         var result = Boolean.Subtract(plate, negative);
        //         
        //         // CSG operation puts the mesh at the world position so need to undo that
        //         var mesh = OffsetMeshVertices(result.mesh, -plate.transform.position);
        //         meshFilter.mesh.Clear();
        //         meshFilter.mesh = mesh;
        //     }
        //     // Clean up the negative
        //     //Destroy(negative);
        // }
        //
        //
        // private Mesh OffsetMeshVertices(Mesh mesh, Vector3 offset)
        // {
        //     Vector3[] vertices = mesh.vertices;
        //     for (int i = 0; i < vertices.Length; i++)
        //     {
        //         vertices[i] += offset;
        //     }
        //     mesh.vertices = vertices;
        //     mesh.RecalculateBounds();
        //     return mesh;
        // }



    }
}