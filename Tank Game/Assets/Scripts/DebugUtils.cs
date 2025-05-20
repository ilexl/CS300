using UnityEngine;

public class DebugUtils : MonoBehaviour
{
    public Vector3 center = Vector3.zero;
    public float radius = 5f;
    public int segments = 36;
    public Color color = Color.green;

    void Update()
    {
        DrawSphere(center, radius, color, segments);
    }

    void DrawSphere(Vector3 position, float radius, Color color, int segments)
    {
        float angleStep = 360f / segments;

        // XY plane
        for (int i = 0; i < segments; i++)
        {
            float angleA = angleStep * i;
            float angleB = angleStep * (i + 1);

            Vector3 pointA = position + new Vector3(Mathf.Cos(Mathf.Deg2Rad * angleA) * radius, Mathf.Sin(Mathf.Deg2Rad * angleA) * radius, 0);
            Vector3 pointB = position + new Vector3(Mathf.Cos(Mathf.Deg2Rad * angleB) * radius, Mathf.Sin(Mathf.Deg2Rad * angleB) * radius, 0);
            Debug.DrawLine(pointA, pointB, color);
        }

        // XZ plane
        for (int i = 0; i < segments; i++)
        {
            float angleA = angleStep * i;
            float angleB = angleStep * (i + 1);

            Vector3 pointA = position + new Vector3(Mathf.Cos(Mathf.Deg2Rad * angleA) * radius, 0, Mathf.Sin(Mathf.Deg2Rad * angleA) * radius);
            Vector3 pointB = position + new Vector3(Mathf.Cos(Mathf.Deg2Rad * angleB) * radius, 0, Mathf.Sin(Mathf.Deg2Rad * angleB) * radius);
            Debug.DrawLine(pointA, pointB, color);
        }

        // YZ plane
        for (int i = 0; i < segments; i++)
        {
            float angleA = angleStep * i;
            float angleB = angleStep * (i + 1);

            Vector3 pointA = position + new Vector3(0, Mathf.Cos(Mathf.Deg2Rad * angleA) * radius, Mathf.Sin(Mathf.Deg2Rad * angleA) * radius);
            Vector3 pointB = position + new Vector3(0, Mathf.Cos(Mathf.Deg2Rad * angleB) * radius, Mathf.Sin(Mathf.Deg2Rad * angleB) * radius);
            Debug.DrawLine(pointA, pointB, color);
        }
    }
}