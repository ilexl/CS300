using UnityEngine;

public static class DebugUtils
{
    public static void DebugDrawSphere(Vector3 center, float radius, Color color, float duration = 0, int segments = 24)
    {
        float angleStep = 360f / segments;

        // XY plane
        for (int i = 0; i < segments; i++)
        {
            float angleA = angleStep * i;
            float angleB = angleStep * (i + 1);

            Vector3 pointA = center + new Vector3(Mathf.Cos(Mathf.Deg2Rad * angleA) * radius, Mathf.Sin(Mathf.Deg2Rad * angleA) * radius, 0);
            Vector3 pointB = center + new Vector3(Mathf.Cos(Mathf.Deg2Rad * angleB) * radius, Mathf.Sin(Mathf.Deg2Rad * angleB) * radius, 0);
            Debug.DrawLine(pointA, pointB, color, duration);
        }

        // XZ plane
        for (int i = 0; i < segments; i++)
        {
            float angleA = angleStep * i;
            float angleB = angleStep * (i + 1);

            Vector3 pointA = center + new Vector3(Mathf.Cos(Mathf.Deg2Rad * angleA) * radius, 0, Mathf.Sin(Mathf.Deg2Rad * angleA) * radius);
            Vector3 pointB = center + new Vector3(Mathf.Cos(Mathf.Deg2Rad * angleB) * radius, 0, Mathf.Sin(Mathf.Deg2Rad * angleB) * radius);
            Debug.DrawLine(pointA, pointB, color, duration);
        }

        // YZ plane
        for (int i = 0; i < segments; i++)
        {
            float angleA = angleStep * i;
            float angleB = angleStep * (i + 1);

            Vector3 pointA = center + new Vector3(0, Mathf.Cos(Mathf.Deg2Rad * angleA) * radius, Mathf.Sin(Mathf.Deg2Rad * angleA) * radius);
            Vector3 pointB = center + new Vector3(0, Mathf.Cos(Mathf.Deg2Rad * angleB) * radius, Mathf.Sin(Mathf.Deg2Rad * angleB) * radius);
            Debug.DrawLine(pointA, pointB, color, duration);
        }
    }
}