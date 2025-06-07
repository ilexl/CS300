using Ballistics;
using System;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Projectile : MaterialObject
{
    private float _diameter;
    private float _length;
    private int _layerMask;
    private float _hpPool;
    
    private Vector3 _previousPos;
    public Rigidbody rb;
    public SphereCollider col;
    [SerializeField]
    public UnityEngine.Material unityMaterial;

    

    public void Start()
    {
        _previousPos = transform.position;
        if (unityMaterial == null) throw new Exception("Material field must be set!");
        _layerMask = LayerMask.GetMask("Armour");   
    }
    public void SetProjectileProperties(float velocityMS, float diameterM, float lengthM, MaterialKey mKey)
    {
        
        _diameter = diameterM;
        _length = lengthM;
        MaterialType = mKey;
        col.radius = _diameter / 2;

        
        float density = Material.Density;  // Density in kg/m³

        float surfaceArea = Mathf.PI * Mathf.Pow(diameterM / 2f, 2);
        float volume = surfaceArea * lengthM;

        float mass = volume * density;
        rb.mass = mass;

        rb.linearVelocity = transform.forward * velocityMS;

        // DeMarre formula constants
        float k = Material.Hardness;
        float n = 1.4f;

        float mOverD2 = mass / (diameterM * diameterM);
        _hpPool = k * Mathf.Sqrt(mOverD2) * Mathf.Pow(velocityMS, n);

        Debug.Log("HP pool (penetration power) is " + _hpPool);

        float maxPenetration = _hpPool / (MaterialDatabase.GetMaterial(MaterialKey.HighCarbonSteel).Hardness * SpallableTankModule.ProtectionMultiplier);
        Debug.Log("Maximum penetration is " + maxPenetration * 1000 + "mm" );
    }
    
    public void FixedUpdate()
    {
        Vector3 to = transform.position - _previousPos;
        float mag = to.magnitude;
        Vector3 dir = to / mag;
        var hits = Physics.RaycastAll(_previousPos, dir, mag, _layerMask);
        
        // Sort hits by distance from the cast pos
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        if (hits.Length > 0)
        {
            try
            {
                foreach (var hit in hits)
                {
                    bool shouldGoNext = PenetratePlate(hit.collider, hit.point, dir);
                    if (!shouldGoNext) return;
                }
                
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }
        
        
        _previousPos = transform.position;
    }

    private bool PenetratePlate(Collider collider, Vector3 entryPoint, Vector3 direction)
    {

        var plateGameObject = collider.gameObject;
        // TODO: Generalize to DamageableComponent once implemented
        var panel = plateGameObject.GetComponent<SpallableTankModule>();
        
        var backCastPos = entryPoint + direction * 10f;
        bool didHit = RaycastUtility.RaycastToSpecificObject(backCastPos, -direction, plateGameObject.transform,  out var secondHit, Mathf.Infinity, _layerMask);
        if (!didHit)
        {
            Debug.Log("Did not hit anything");
            return true;
        }
        Vector3 exitPoint = secondHit.point;
        
        Debug.DrawLine(entryPoint, secondHit.point, Color.red, 50);
        float thickness = Vector3.Dot(direction, exitPoint - entryPoint );
        Debug.Log("LOS thickness: " + thickness);
        var protection = thickness * panel.Material.Hardness * SpallableTankModule.ProtectionMultiplier;
        _hpPool -= protection;
        Debug.Log(_hpPool);
        if (_hpPool <= 0)
        {
            // Non penetration, projectile should go away
            Destroy(gameObject);
            return false;
        }

        panel.PostPenetration(entryPoint, exitPoint, thickness, rb.linearVelocity, Vector3.zero, _diameter);
        return true;
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