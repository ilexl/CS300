using UnityEngine;

/// <summary>
/// Utility class providing helpful debug drawing methods.
/// Includes a method to visualize a sphere in the scene view by drawing
/// three circles on the XY, XZ, and YZ planes using Debug.DrawLine.
/// This aids in debugging spatial areas or ranges during development.
/// </summary>
public static class DebugUtils
{
    /// <summary>
    /// Draws a wireframe sphere using Debug.DrawLine by approximating circles
    /// on the XY, XZ, and YZ planes around the given center.
    /// Useful for visual debugging of spherical areas in the editor or during play.
    /// </summary>
    /// <param name="center">Center point of the sphere.</param>
    /// <param name="radius">Radius of the sphere.</param>
    /// <param name="color">Color of the debug lines.</param>
    /// <param name="duration">How long the lines should be visible.</param>
    /// <param name="segments">Number of line segments per circle for smoothness.</param>
    public static void DebugDrawSphere(Vector3 center, float radius, Color color, float duration = 0, int segments = 24)
    {
        float angleStep = 360f / segments;

        // Draw circle on XY plane by connecting points along circumference
        for (int i = 0; i < segments; i++)
        {
            float angleA = angleStep * i;
            float angleB = angleStep * (i + 1);

            Vector3 pointA = center + new Vector3(Mathf.Cos(Mathf.Deg2Rad * angleA) * radius, Mathf.Sin(Mathf.Deg2Rad * angleA) * radius, 0);
            Vector3 pointB = center + new Vector3(Mathf.Cos(Mathf.Deg2Rad * angleB) * radius, Mathf.Sin(Mathf.Deg2Rad * angleB) * radius, 0);
            Debug.DrawLine(pointA, pointB, color, duration);
        }

        // Draw circle on XZ plane similarly
        for (int i = 0; i < segments; i++)
        {
            float angleA = angleStep * i;
            float angleB = angleStep * (i + 1);

            Vector3 pointA = center + new Vector3(Mathf.Cos(Mathf.Deg2Rad * angleA) * radius, 0, Mathf.Sin(Mathf.Deg2Rad * angleA) * radius);
            Vector3 pointB = center + new Vector3(Mathf.Cos(Mathf.Deg2Rad * angleB) * radius, 0, Mathf.Sin(Mathf.Deg2Rad * angleB) * radius);
            Debug.DrawLine(pointA, pointB, color, duration);
        }

        // Draw circle on YZ plane similarly
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