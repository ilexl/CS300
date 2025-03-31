using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
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

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
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

        // Get the target's world position (the tank)
        Vector3 targetPosition = target.position + offset;

        // Log the tank's world position to see where the camera is supposed to pivot
        Debug.Log($"Tank World Position: {target.position}");

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 desiredPosition = targetPosition - (rotation * Vector3.forward * distance);

        // Log the desired camera position
        Debug.Log($"Desired Camera Position: {desiredPosition}");

        // Smoothly move the camera towards the desired position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Make the camera look at the target
        transform.LookAt(target.position + offset);
    }
}
