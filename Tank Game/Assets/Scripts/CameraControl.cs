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

    private bool sniperModeCOV = false; // handles changes between sniper mode and normal mode ONLY


    void Start()
    {
        
    }

    void Update()
    {
        currentX += Input.GetAxis("Mouse X") * sensitivity;
        currentY -= Input.GetAxis("Mouse Y") * sensitivity;
        currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);

        // Handle Zooming
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
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
        if (sniperModeCOV is false)
        {
            Camera.main.fieldOfView = SniperFOV;
            sniperModeCOV = true;

            // Attach camera to turret (move camera pos)
            transform.position = target.GetComponent<TankMovement>().GetSniperCameraTransform().position;
            transform.LookAt(target.GetComponent<TankMovement>().GetAimPoint());

            // Hide models of tank while zoomed in
            target.GetComponent<TankVisuals>().HideTankSniperMode();
        }



        // Allow scrolling to change zoom (FOV)
        // TODO: allow scrolling for zoom FOV

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

            // Attach camera to above again (move camera pos)
            // TODO: Move camera faster as there is currently a delay

            // Hide models of tank while zoomed in
            target.GetComponent<TankVisuals>().ShowTankNormal();
        }
        // Get the target's world position (the tank)
        Vector3 targetPosition = target.position + offset;

        // Log the tank's world position to see where the camera is supposed to pivot
        // Debug.Log($"Tank World Position: {target.position}");

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 desiredPosition = targetPosition - (rotation * Vector3.forward * distance);

        // Log the desired camera position
        // Debug.Log($"Desired Camera Position: {desiredPosition}");

        // Smoothly move the camera towards the desired position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Make the camera look at the target
        transform.LookAt(target.position + offset);
    }
}
