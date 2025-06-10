using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraControl : MonoBehaviour
{
    public Transform target; // Assign the tank here
    public float distance = 5f;
    public float minDistance = 2f;
    public float maxDistance = 10f;
    public float zoomSpeed = 2f;
    public float sensitivity = 2f;
    public float minYAngle = -30f;
    public float maxYAngle = 60f;
    public Vector3 offset = new Vector3(0, 2f, 0); // Offset to position camera higher
    public float followSpeed = 10f; // Speed at which the camera follows the target

    private float currentX = 0f;
    private float currentY = 0f;
    private float NomralFOV = 60f;
    private float SniperFOV = 40f;
    private float sniperFOVMin = 20f; // Min FOV in sniper mode
    private float sniperFOVMax = 60f; // Max FOV in sniper mode
    private float sniperZoomSpeed = 20f; // Speed of zooming FOV in sniper mode

    private bool sniperModeCOV = false; // handles changes between sniper mode and normal mode ONLY

    void Update()
    {
        var input = Input.mousePositionDelta;
        currentX += input.x * sensitivity;
        currentY -= input.y * sensitivity;
        currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);

        if (target != null && target.GetComponent<TankMovement>() != null &&
            target.GetComponent<TankMovement>().SniperMode)
        {
            // Handle zooming via FOV in Sniper Mode
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            Camera.main.fieldOfView -= scroll * sniperZoomSpeed;
            Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, sniperFOVMin, sniperFOVMax);
        }
        else
        {
            // Handle Zooming via distance in normal mode
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;
        if (target.GetComponent<TankMovement>() is null)
        {
            CameraMovement(); // spectate only if no player controls i.e. no tank movement script
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

    void SniperMode()
    {
        // Sniper Mode

        // Change FOV to zoom (detect change of states)
        if (!sniperModeCOV)
        {
            Camera.main.fieldOfView = SniperFOV;
            sniperModeCOV = true;

            // Attach camera to turret (move camera pos)
            transform.position = target.GetComponent<TankMovement>().GetSniperCameraTransform().position;
            transform.LookAt(target.GetComponent<TankMovement>().GetAimPoint());

            // Hide models of tank while zoomed in
            target.GetComponent<TankVisuals>().HideTankSniperMode();
        }

        // Move camera
        transform.position = target.GetComponent<TankMovement>().GetSniperCameraTransform().position; // always update position
        transform.localRotation = Quaternion.Euler(currentY, currentX, 0f);
    }

    void CameraMovement()
    {
        if (sniperModeCOV)
        {
            Camera.main.fieldOfView = NomralFOV;
            sniperModeCOV = false;

            // Hide models of tank while zoomed in
            target.GetComponent<TankVisuals>().ShowTankNormal();
        }

        Vector3 targetPosition = target.position + offset;
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 desiredPosition = targetPosition - (rotation * Vector3.forward * distance);

        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        transform.LookAt(target.position + offset);
    }
}
