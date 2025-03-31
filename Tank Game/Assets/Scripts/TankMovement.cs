using UnityEngine;

public class TankMovement : MonoBehaviour
{
    public Transform hull;
    public Transform turret;
    public Transform cannon;
    public Camera playerCamera;
    public float moveSpeed = 5f;
    public float rotationSpeed = 60f;
    public float turretRotationSpeed = 80f;
    public float cannonElevationSpeed = 30f;
    public float minCannonAngle = -10f;
    public float maxCannonAngle = 20f;
    public float aimSmoothSpeed = 5f;

    private float cannonAngle = 0f;
    private Vector3 aimPoint;

    private void Awake()
    {
        playerCamera = Camera.main;
    }

    void Update()
    {
        HandleHullMovement();
        UpdateAimPoint();
        HandleTurretRotation();
        HandleCannonElevation();
    }

    void HandleHullMovement()
    {
        float moveInput = Input.GetAxis("Vertical");
        float rotateInput = Input.GetAxis("Horizontal");

        hull.Translate(Vector3.forward * moveInput * moveSpeed * Time.deltaTime);
        hull.Rotate(Vector3.up * rotateInput * rotationSpeed * Time.deltaTime);
    }

    void UpdateAimPoint()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            aimPoint = hit.point;
        }
        else
        {
            aimPoint = playerCamera.transform.position + playerCamera.transform.forward * 1000f;
        }
    }

    void HandleTurretRotation()
    {
        Vector3 direction = (aimPoint - turret.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        turret.rotation = Quaternion.Slerp(turret.rotation, lookRotation, aimSmoothSpeed * Time.deltaTime);
    }

    void HandleCannonElevation()
    {
        Vector3 direction = (aimPoint - cannon.position).normalized;
        float targetAngle = Mathf.Asin(direction.y) * Mathf.Rad2Deg;
        cannonAngle = Mathf.Clamp(targetAngle, minCannonAngle, maxCannonAngle);
        cannon.localRotation = Quaternion.Lerp(cannon.localRotation, Quaternion.Euler(cannonAngle, 0, 0), aimSmoothSpeed * Time.deltaTime);
    }
}
