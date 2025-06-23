using UnityEngine;

/// <summary>
/// A user-controlled orbit camera for the main menu scene.
/// Allows rotation and zooming around a target (e.g., a display tank).
/// </summary>
public class CameraMainMenu : MonoBehaviour
{
    [Header("Target & Positioning")]
    public Transform target;                      // The object to orbit around
    public Vector3 offset = new Vector3(0, 2f, 0); // Elevates the camera above the target

    [Header("Zoom Settings")]
    public float distance = 5f;
    public float minDistance = 2f;
    public float maxDistance = 10f;
    public float zoomSpeed = 2f;

    [Header("Rotation Settings")]
    public float sensitivity = 2f;
    public float minYAngle = -30f;
    public float maxYAngle = 60f;

    [Header("Follow Settings")]
    public float followSpeed = 10f; // Controls smooth camera movement

    private float currentX = 0f;
    private float currentY = 0f;

    public static bool mouseBusy = false; // Used by other UI elements to prevent camera control

    /// <summary>
    /// Unlocks the mouse cursor for menu interaction on start.
    /// </summary>
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        mouseBusy = false;
    }

    /// <summary>
    /// Handles user input for rotating and zooming the camera.
    /// Ignores input if mouse is locked by another UI element.
    /// </summary>
    void Update()
    {
        if (mouseBusy) return;

        // Allow orbit rotation when mouse is active or manually locked
        if (Cursor.lockState == CursorLockMode.Locked || Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                currentX += Input.GetAxis("Mouse X") * sensitivity;
                currentY -= Input.GetAxis("Mouse Y") * sensitivity;
            }

            // Prevent camera from flipping upside down
            currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);
        }

        // Handle scroll wheel zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

    /// <summary>
    /// Updates the camera's position and orientation after all other updates.
    /// Ensures smooth follow and accurate aiming at the target.
    /// </summary>
    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 desiredPosition = targetPosition - (rotation * Vector3.forward * distance);

        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        transform.LookAt(target.position + offset);
    }
}
