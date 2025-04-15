using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TankMovement : MonoBehaviour
{
    [SerializeField] GameObject hull;
    [SerializeField] List<GameObject> turrets;
    [SerializeField] List<GameObject> cannons;
    [SerializeField] Camera playerCamera;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 60f;
    [SerializeField] float turretRotationSpeed = 80f;
    [SerializeField] float cannonElevationSpeed = 30f;
    [SerializeField] float minCannonAngle = -10f;
    [SerializeField] float maxCannonAngle = 20f;
    [SerializeField] bool canMove = true;

    public bool CanMove() { return canMove; }
    public void SetCanMove(bool set) { canMove = set; }

    private float cannonAngle = 0f;
    private Vector3 aimPoint;

    public void UpdateTank(TankVarients tank, GameObject hull, List<GameObject> turrets, List<GameObject> cannons)
    {
        this.hull = hull;
        this.turrets = turrets;
        this.cannons = cannons;
    }

    void Awake()
    {
        playerCamera = Camera.main;
    }

    void Update()
    {
        if (canMove is false) { return; }
        HandleHullMovement();
        UpdateAimPoint();
        HandleTurretRotation();
        HandleCannonElevation();
    }

    void HandleHullMovement()
    {
        float moveInput = Input.GetAxis("Vertical");
        float rotateInput = Input.GetAxis("Horizontal");

        hull.transform.Translate(Vector3.forward * moveInput * moveSpeed * Time.deltaTime);
        hull.transform.Rotate(Vector3.up * rotateInput * rotationSpeed * Time.deltaTime);
    }

    void UpdateAimPoint()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            aimPoint = hit.point;
            Debug.DrawLine(playerCamera.transform.position, hit.point, Color.red);
        }
        else
        {
            aimPoint = playerCamera.transform.position + playerCamera.transform.forward * 1000f;
            Debug.DrawLine(playerCamera.transform.position, aimPoint, Color.green);
        }

        // Log the aim point for debugging purposes
        Debug.Log($"Aim Point: {aimPoint}");
    }

    void HandleTurretRotation()
    {
        foreach(GameObject turret in turrets)
        {
            // Calculate the direction to the aim point without considering the Y axis (to prevent vertical rotation due to slope)
            Vector3 targetDirection = aimPoint - turret.transform.position;
            targetDirection.y = 0; // Ignore the Y-axis to keep the turret's rotation horizontal

            // Rotate the turret towards the target direction
            if (targetDirection.sqrMagnitude > 0.01f) // If the target direction is not too small
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                turret.transform.rotation = Quaternion.Slerp(turret.transform.rotation, targetRotation, turretRotationSpeed * Time.deltaTime);
            }
            turret.transform.localEulerAngles = new Vector3(0, turret.transform.localEulerAngles.y, 0);
        }
    }

    void HandleCannonElevation()
    {
        foreach (GameObject cannon in cannons)
        {
            Vector3 direction = (aimPoint - cannon.transform.position).normalized;
            float targetAngle = Mathf.Asin(direction.y) * Mathf.Rad2Deg;
            cannonAngle = Mathf.Clamp(targetAngle, minCannonAngle, maxCannonAngle);
            cannon.transform.localRotation = Quaternion.Lerp(cannon.transform.localRotation, Quaternion.Euler(cannonAngle, 0, 0), cannonElevationSpeed * Time.deltaTime);
        }
    }
}
