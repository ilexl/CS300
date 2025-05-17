using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] Transform sniperCameraPos;
    [SerializeField] Transform debugAimObject;
    TankVarients currentTank;

    private Vector3 aimPoint;
    public bool SniperMode
    {
        get { return sniperMode; } // public read only
    }

    public Transform GetSniperCameraTransform() { return sniperCameraPos; }
    public Vector3 GetAimPoint() { return aimPoint; }

    private bool sniperMode; // only changable from this script

    public bool CanMove() { return canMove; }
    public void SetCanMove(bool set) { canMove = set; }


    public void UpdateTank(TankVarients tank, GameObject hull, List<GameObject> turrets, List<GameObject> cannons, GameObject SniperCameraPos)
    {
        currentTank = tank;
        this.hull = hull;
        this.turrets = turrets;
        this.cannons = cannons;
        this.sniperCameraPos = SniperCameraPos.transform;
    }

    public void SetPlayerCamera(Camera camera)
    {
        playerCamera = camera;
    }

    void Update()
    {
        if (canMove is false) { return; }
        if (GetComponent<Player>().LocalPlayer == false) { return; }
        if (GetComponent<Player>().TankVarient == null) { return; }
        if (hull == null || turrets == null || cannons == null || sniperCameraPos == null) 
        {
            if (currentTank != null) { FixTankRunTime(); }
            return; 
        }
        HandleHullMovement();
        SniperCheck();
        UpdateAimPoint();
        HandleTurretRotation();
        HandleCannonElevation();
    }

    void FixTankRunTime()
    {
        GetComponent<Player>().ChangeTank(GetComponent<Player>().TankVarient);
        GetComponent<TankVisuals>().RefreshList();
    }

    void HandleHullMovement()
    {
        if (hull == null)
        {
            Debug.LogWarning("Tank Movement missing assigned variables...");
            return;
        }
        float moveInput = Input.GetAxis("Vertical");
        float rotateInput = Input.GetAxis("Horizontal");

        if(moveInput < 0)
        {
            rotateInput *= -1;
        }

        hull.transform.Translate(Vector3.forward * moveInput * moveSpeed * Time.deltaTime);
        hull.transform.Rotate(Vector3.up * rotateInput * rotationSpeed * Time.deltaTime);
    }

    void UpdateAimPoint()
    {
        if((playerCamera is null || playerCamera == null) && GetComponent<Player>().LocalPlayer == true) 
        {
            playerCamera = Camera.main;
        }
        if(playerCamera is null || playerCamera == null) { return; }
        int layerMask = ~((1 << 2) | (1 << 10)); // hit everything but layer 10 and 2
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, layerMask))
        {
            aimPoint = hit.point;
            Debug.DrawLine(playerCamera.transform.position, hit.point, Color.red);
            if(debugAimObject is null || debugAimObject == null) { return; }
            
            debugAimObject.position = aimPoint;
        }
        else
        {
            aimPoint = playerCamera.transform.position + playerCamera.transform.forward * 1000f;
            Debug.DrawLine(playerCamera.transform.position, aimPoint, Color.green);
            if (debugAimObject is null || debugAimObject == null) { return; }
            
            debugAimObject.position = aimPoint;
        }

        // Log the aim point for debugging purposes
        // Debug.Log($"Aim Point: {aimPoint}");
    }

    void HandleTurretRotation()
    {
        if(turrets == null)
        { 
            Debug.LogWarning("Tank Movement missing assigned variables...");
            return;
        }
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
        if (cannons == null)
        {
            Debug.LogWarning("Tank Movement missing assigned variables...");
            return;
        }
        foreach (GameObject cannon in cannons)
        {
            Transform cannonBarrel = cannon.transform.GetChild(0);
            Vector3 direction = aimPoint - cannonBarrel.position;
            Vector3 localDirection = cannonBarrel.parent.InverseTransformDirection(direction.normalized);

            float pitchAngle = -Mathf.Atan2(localDirection.y, new Vector2(localDirection.x, localDirection.z).magnitude) * Mathf.Rad2Deg;
            pitchAngle = Mathf.Clamp(pitchAngle, minCannonAngle, maxCannonAngle);

            Quaternion targetRotation = Quaternion.Euler(pitchAngle, 0, 0); // Or Euler(0, 0, pitchAngle) depending on your axis
            cannonBarrel.localRotation = Quaternion.Lerp(cannonBarrel.localRotation, targetRotation, cannonElevationSpeed * Time.deltaTime);




            if (debugAimObject is not null)
            {
                // Get cannons child
                Transform cannonAimStart = cannon.transform.GetChild(0);
                if (cannonAimStart == null)
                {

                }
                Ray ray = new Ray(cannonAimStart.position, cannon.transform.GetChild(0).forward);
                int layerMask = ~((1 << 2) | (1 << 10)); // hit everything but layer 10 and 2
                if (Physics.Raycast(ray, out RaycastHit currentHit, 1000f, layerMask))
                {
                    Vector3 currentAimPoint = currentHit.point;
                    Debug.DrawLine(cannonAimStart.position, currentAimPoint, Color.yellow);
                }
                else
                {
                    Vector3 currentAimPoint = cannonAimStart.position + cannon.transform.forward * 1000f;
                    Debug.DrawLine(cannonAimStart.position, currentAimPoint, Color.yellow);
                }
            }

            
        }
    }

    void SniperCheck()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            sniperMode = !sniperMode;
        }
    }

    public GameObject GetCannon(int cannonIndex)
    {
        return cannons[cannonIndex];
    }
}
