using System.Collections.Generic;
using UnityEngine;

public static class ProjectileDamageMeshGenerator
{

/// <summary>
    /// Generates a cylinder of given radius and resolution between pointA and pointB,
    /// but with each end‐cap center offset outward by radius along the axis.
    /// </summary>
    public static Mesh Generate(Vector3 pointA, Vector3 pointB, float radius, int resolution)
{
    var mesh = new Mesh { name = "PointedCapsCylinder_MidRing" };

    // 1) Compute axis & offset apex positions
    Vector3 axis  = (pointB - pointA).normalized;
    Vector3 apexA = pointA - axis * radius;
    Vector3 apexB = pointB + axis * radius;

    // 2) Build basis (tangent, bitangent)
    Vector3 tmp     = (Mathf.Abs(Vector3.Dot(axis, Vector3.up)) < .99f) ? Vector3.up : Vector3.right;
    Vector3 tangent = Vector3.Cross(axis, tmp).normalized;
    Vector3 bitan   = Vector3.Cross(axis, tangent);

    // 3) Build the three rings (pointA, midpoint, pointB)
    var verts = new List<Vector3>();
    var norms = new List<Vector3>();
    var uvs   = new List<Vector2>();

    for (int ring = 0; ring < 3; ring++)
    {
        Vector3 center = Vector3.Lerp(pointA, pointB, ring / 2f);
        float   vrow   = ring / 2f;
        for (int i = 0; i < resolution; i++)
        {
            float theta = (i / (float)resolution) * Mathf.PI * 2f;
            Vector3 dir = Mathf.Cos(theta) * tangent + Mathf.Sin(theta) * bitan;
            verts.Add(center + dir * radius);
            norms.Add(dir);                          // side normal
            uvs.Add(new Vector2(i / (float)resolution, vrow));
        }
    }

    // 4) Add the two apex vertices
    int apexIndexA = verts.Count;
    verts.Add(apexA);
    norms.Add(-axis);
    uvs.Add(new Vector2(0.5f, 0f));

    int apexIndexB = verts.Count;
    verts.Add(apexB);
    norms.Add(axis);
    uvs.Add(new Vector2(0.5f, 1f));

    // 5) Build triangle list
    var tris = new List<int>();

    // — Sides (ring0 to ring1, and ring1 to ring2) —
    for (int ring = 0; ring < 2; ring++)
    {
        int ringOffset0 = ring * resolution;
        int ringOffset1 = (ring + 1) * resolution;

        for (int i = 0; i < resolution; i++)
        {
            int next = (i + 1) % resolution;
            int a0 = ringOffset0 + i;
            int a1 = ringOffset0 + next;
            int b0 = ringOffset1 + i;
            int b1 = ringOffset1 + next;

            // Triangle winding (you can flip these pairs if you need)
            tris.Add(a0); tris.Add(a1); tris.Add(b0);
            tris.Add(a1); tris.Add(b1); tris.Add(b0);
        }
    }

    // — Cap A cone —
    for (int i = 0; i < resolution; i++)
    {
        int next = (i + 1) % resolution;
        tris.Add(apexIndexA);
        tris.Add(next);
        tris.Add(i);
    }

    // — Cap B cone —
    int ring2Offset = 2 * resolution;
    for (int i = 0; i < resolution; i++)
    {
        int next = (i + 1) % resolution;
        tris.Add(apexIndexB);
        tris.Add(ring2Offset + i);
        tris.Add(ring2Offset + next);
    }

    // 6) Finalize mesh
    mesh.SetVertices(verts);
    mesh.SetNormals(norms);
    mesh.SetUVs(0, uvs);
    mesh.SetTriangles(tris, 0);
    mesh.RecalculateBounds();

    return mesh;
}


}
