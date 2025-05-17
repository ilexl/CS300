using UnityEngine;

public class RaycastUtility : MonoBehaviour
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
        Transform targetObject,
        out RaycastHit hit,
        float maxDistance = Mathf.Infinity,
        LayerMask? layerMask = null)
    {
        // Perform RaycastAll
        RaycastHit[] hits;

        if (layerMask.HasValue)
        {
            hits = Physics.RaycastAll(origin, direction, maxDistance, layerMask.Value);
        }
        else
        {
            hits = Physics.RaycastAll(origin, direction, maxDistance);
        }

        // Iterate through all hits
        foreach (RaycastHit _hit in hits)
        {
            // Check if the hit object matches the target
            if (_hit.transform == targetObject)
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

