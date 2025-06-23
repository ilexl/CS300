using UnityEngine;

/// <summary>
/// Handles third-person and sniper camera behavior for a tank-based game.
/// Supports smooth follow, rotation, distance zooming, and sniper-mode FOV zooming.
/// </summary>
public class CameraControl : MonoBehaviour
{
    [Header("Target & Offset")]
    public Transform target; // The tank or object to follow
    public Vector3 offset = new Vector3(0, 2f, 0); // Elevation of camera above the tank

    [Header("Camera Rotation")]
    public float sensitivity = 2f;
    public float minYAngle = -30f;
    public float maxYAngle = 60f;

    [Header("Normal Mode Zoom")]
    public float distance = 5f;
    public float minDistance = 2f;
    public float maxDistance = 10f;
    public float zoomSpeed = 2f;

    [Header("Follow Behavior")]
    public float followSpeed = 10f;

    [Header("Sniper Mode FOV")]
    private float NomralFOV = 60f;
    private float SniperFOV = 40f;
    private float sniperFOVMin = 20f; 
    private float sniperFOVMax = 60f; 
    private float sniperZoomSpeed = 20f;

    private float currentY = 0f;
    private float currentX = 0f;
    private bool sniperModeCOV = false; // Used to detect transition between normal and sniper mode

    /// <summary>
    /// Handles user input for rotating the camera and zooming.
    /// Chooses zoom behavior based on whether sniper mode is active.
    /// </summary>
    void Update()
    {
        // Mouse movement for camera orbiting
        currentX += Input.GetAxis("Mouse X") * sensitivity;
        currentY -= Input.GetAxis("Mouse Y") * sensitivity;
        currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);

        if (target != null && target.GetComponent<TankMovement>()?.SniperMode == true)
        {
            // Scroll wheel adjusts field of view in sniper mode
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            Camera.main.fieldOfView -= scroll * sniperZoomSpeed;
            Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, sniperFOVMin, sniperFOVMax);
        }
        else
        {
            // Scroll wheel adjusts camera distance in normal mode
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    /// <summary>
    /// Chooses between normal follow and sniper mode each frame.
    /// Ensures position/rotation updates happen after all movement.
    /// </summary>
    void LateUpdate()
    {
        if (target == null) return;

        if (target.GetComponent<TankMovement>() is null)
        {
            // If target isn't controlled by player (spectator cam etc.)
            CameraMovement(); 
            return;
        }

        if (target.GetComponent<TankMovement>().SniperMode)
        {
            SniperMode();
        }
        else
        {
            CameraMovement();
        }
    }

    /// <summary>
    /// Activates and maintains sniper mode camera behavior.
    /// Zooms in via FOV, positions camera near barrel, and hides tank visuals.
    /// </summary>
    void SniperMode()
    {
        if (!sniperModeCOV)
        {
            Camera.main.fieldOfView = SniperFOV;
            sniperModeCOV = true;

            // Snap to sniper view point and hide tank mesh
            transform.position = target.GetComponent<TankMovement>().GetSniperCameraTransform().position;
            transform.LookAt(target.GetComponent<TankMovement>().GetAimPoint());
            target.GetComponent<TankVisuals>().HideTankSniperMode();
        }

        // Continuously align with sniper camera position and user input
        transform.position = target.GetComponent<TankMovement>().GetSniperCameraTransform().position; // always update position
        transform.localRotation = Quaternion.Euler(currentY, currentX, 0f);
    }

    /// <summary>
    /// Handles standard third-person camera following and orbiting.
    /// Smoothly follows target, allows mouse rotation and zooming by distance.
    /// </summary>
    void CameraMovement()
    {
        // Reset sniper mode if it was previously active
        if (sniperModeCOV)
        {
            Camera.main.fieldOfView = NomralFOV;
            sniperModeCOV = false;
            target.GetComponent<TankVisuals>().ShowTankNormal();
        }

        Vector3 targetPosition = target.position + offset;
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 desiredPosition = targetPosition - (rotation * Vector3.forward * distance);

        // Smooth follow and aim
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        transform.LookAt(target.position + offset);
    }
}
