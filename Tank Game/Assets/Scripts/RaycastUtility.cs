using UnityEngine;

/// <summary>
/// Provides utility methods for performing raycasts with additional filtering,
/// such as targeting specific GameObjects using RaycastAll.
/// </summary>
public static class RaycastUtility
{
    /// <summary>
    /// Checks if a specific object was hit by a ray using RaycastAll.
    /// </summary>
    /// <param name="origin">The starting point of the ray.</param>
    /// <param name="direction">The direction of the ray.</param>
    /// <param name="targetObject">The specific object to detect.</param>
    /// <param name="maxDistance">The maximum distance of the raycast.</param>
    /// <param name="layerMask">Optional LayerMask to limit hits to specific layers.</param>
    /// <returns>True if the target object was hit; otherwise, false.</returns>
    public static bool RaycastToSpecificObject(
        Vector3 origin,
        Vector3 direction,
        GameObject targetObject,
        out RaycastHit hit,
        float maxDistance = Mathf.Infinity,
        int layerMask = -1)
    {
        // Perform RaycastAll
        RaycastHit[] hits;

        if (layerMask != -1)
        {
            hits = Physics.RaycastAll(origin, direction, maxDistance, layerMask);
        }
        else
        {
            hits = Physics.RaycastAll(origin, direction, maxDistance);
        }

        // Iterate through all hits
        foreach (RaycastHit _hit in hits)
        {
            // Check if the hit object matches the target
            
            if (_hit.collider.gameObject == targetObject)
            {
                hit = _hit;
                return true; // Target object was hit
            }
        }

        // Target object was not hit
        hit = default;
        return false;
    }
}

