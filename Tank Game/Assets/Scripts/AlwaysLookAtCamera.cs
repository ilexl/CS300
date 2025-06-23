using UnityEngine;

/// <summary>
/// Keeps the object's rotation aligned with the main camera every frame.
/// Useful for world-space UI or indicators that must always face the player.
/// </summary>
public class AlwaysLookAtCamera : MonoBehaviour
{

    /// <summary>
    /// Rotates the object to match the main camera's rotation each frame.
    /// If no camera is found, the update is skipped.
    /// </summary>
    void Update()
    {
        if(Camera.main is null) { return; }
        transform.rotation = Camera.main.transform.rotation;
    }
}
