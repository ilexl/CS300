using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TankMovement : NetworkBehaviour
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
    private bool sniperMode;

    private Vector3 lastServerPosition;
    private Quaternion lastServerRotation;
    private float correctionThreshold = 1.5f;

    public bool SniperMode => sniperMode;
    public Transform GetSniperCameraTransform() => sniperCameraPos;
    public Vector3 GetAimPoint() => aimPoint;

    public bool CanMove() => canMove;
    public void SetCanMove(bool set) => canMove = set;

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
        if(IsOwner == false) { return; }
        if(canMove == false) { return; }

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

    void LateUpdate()
    {
        if (IsServer && !IsOwner)
        {
            ServerValidateClientMovement();
        }
    }

    void FixTankRunTime()
    {
        GetComponent<Player>().ChangeTank(GetComponent<Player>().TankVarient);
        GetComponent<TankVisuals>().RefreshList();
    }

    void HandleHullMovement()
    {
        float moveInput = Input.GetAxis("Vertical");
        float rotateInput = Input.GetAxis("Horizontal");

        if (moveInput < 0) rotateInput *= -1;

        hull.transform.Translate(Vector3.forward * moveInput * moveSpeed * Time.deltaTime);
        hull.transform.Rotate(Vector3.up * rotateInput * rotationSpeed * Time.deltaTime);

        SubmitPositionToServerServerRpc(hull.transform.position, hull.transform.rotation);
    }

    [ServerRpc]
    void SubmitPositionToServerServerRpc(Vector3 position, Quaternion rotation)
    {
        lastServerPosition = position;
        lastServerRotation = rotation;
    }

    void ServerValidateClientMovement()
    {
        float distance = Vector3.Distance(hull.transform.position, lastServerPosition);
        if (distance > correctionThreshold)
        {
            hull.transform.position = lastServerPosition;
            hull.transform.rotation = lastServerRotation;
            Debug.LogWarning($"Correcting client movement. Distance was {distance}.");
        }
    }

    void UpdateAimPoint()
    {
        if ((playerCamera == null) && IsOwner) playerCamera = Camera.main;
        if (playerCamera == null) return;

        int layerMask = ~((1 << 2) | (1 << 10));
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, layerMask))
        {
            aimPoint = hit.point;
            Debug.DrawLine(playerCamera.transform.position, hit.point, Color.red);
            if (debugAimObject != null) debugAimObject.position = aimPoint;
        }
        else
        {
            aimPoint = playerCamera.transform.position + playerCamera.transform.forward * 1000f;
            Debug.DrawLine(playerCamera.transform.position, aimPoint, Color.green);
            if (debugAimObject != null) debugAimObject.position = aimPoint;
        }
    }

    void HandleTurretRotation()
    {
        if (turrets == null) return;

        foreach (GameObject turret in turrets)
        {
            Vector3 targetDirection = aimPoint - turret.transform.position;
            targetDirection.y = 0;

            if (targetDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                turret.transform.rotation = Quaternion.Slerp(turret.transform.rotation, targetRotation, turretRotationSpeed * Time.deltaTime);
            }

            turret.transform.localEulerAngles = new Vector3(0, turret.transform.localEulerAngles.y, 0);
        }
    }

    void HandleCannonElevation()
    {
        if (cannons == null) return;

        foreach (GameObject cannon in cannons)
        {
            Transform cannonBarrel = cannon.transform.GetChild(0);
            Vector3 direction = aimPoint - cannonBarrel.position;
            Vector3 localDirection = cannonBarrel.parent.InverseTransformDirection(direction.normalized);

            float pitchAngle = -Mathf.Atan2(localDirection.y, new Vector2(localDirection.x, localDirection.z).magnitude) * Mathf.Rad2Deg;
            pitchAngle = Mathf.Clamp(pitchAngle, minCannonAngle, maxCannonAngle);

            Quaternion targetRotation = Quaternion.Euler(pitchAngle, 0, 0);
            cannonBarrel.localRotation = Quaternion.Lerp(cannonBarrel.localRotation, targetRotation, cannonElevationSpeed * Time.deltaTime);

            if (debugAimObject != null)
            {
                Transform cannonAimStart = cannon.transform.GetChild(0);
                Ray ray = new Ray(cannonAimStart.position, cannon.transform.GetChild(0).forward);
                int layerMask = ~((1 << 2) | (1 << 10));
                if (Physics.Raycast(ray, out RaycastHit currentHit, 1000f, layerMask))
                {
                    Debug.DrawLine(cannonAimStart.position, currentHit.point, Color.yellow);
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
        if (Input.GetKeyDown(KeyCode.LeftShift)) sniperMode = !sniperMode;
    }

    public GameObject GetCannon(int cannonIndex) => cannons[cannonIndex];
}
