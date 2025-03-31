using System.Collections.Generic;
using UnityEngine;

public class TankMovement : MonoBehaviour
{
    [SerializeField] Transform hull;
    [SerializeField] Transform turret;
    [SerializeField] Transform cannon;
    [SerializeField] Camera playerCamera;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 60f;
    [SerializeField] float turretRotationSpeed = 80f;
    [SerializeField] float cannonElevationSpeed = 30f;
    [SerializeField] float minCannonAngle = -10f;
    [SerializeField] float maxCannonAngle = 20f;
    [SerializeField] float aimSmoothSpeed = 5f;

    private float cannonAngle = 0f;
    private Vector3 aimPoint;

    public void UpdateTank(TankVarients tank, GameObject hull, List<GameObject> turrets, List<GameObject> cannons)
    {
        this.hull = hull.transform;
        this.turret = turrets[0].transform;
        this.cannon = cannons[0].transform;
    }

    void Awake()
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
