using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Handles player-controlled tank movement, aiming, turret rotation,
/// cannon elevation, syncing with server, and sniper mode.
/// </summary>
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
    [SerializeField] Transform sniperCameraPos;
    [SerializeField] Transform debugAimObject;
    TankVarients currentTank;

    private Vector3 aimPoint;
    private bool sniperMode;

    private Vector3 lastServerPosition;
    private Quaternion lastServerRotation;
    private float correctionThreshold = 5f;

    private List<Quaternion> turretRotations = new List<Quaternion>();
    private List<Quaternion> cannonRotations = new List<Quaternion>();

    private NetworkVariable<bool> canMove = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    /// <summary>
    /// Whether sniper mode is currently enabled.
    /// </summary>
    public bool SniperMode => sniperMode;

    /// <summary>
    /// Gets the world transform of the sniper camera position.
    /// </summary>
    public Transform GetSniperCameraTransform() => sniperCameraPos;

    /// <summary>
    /// Returns the current aim point in world space.
    /// </summary>
    public Vector3 GetAimPoint() => aimPoint;

    /// <summary>
    /// Returns whether the tank can currently move.
    /// </summary>
    public bool CanMove() => canMove.Value;

    /// <summary>
    /// Sets whether the tank can move. Intended for internal server control.
    /// </summary>
    public void SetCanMove(bool set) => canMove.Value = set; // kept to avoid logic break (DO NOT USE directly in gameplay)

    /// <summary>
    /// Updates this tank's components (used when switching tanks).
    /// </summary>
    public void UpdateTank(TankVarients tank, GameObject hull, List<GameObject> turrets, List<GameObject> cannons, GameObject SniperCameraPos)
    {
        currentTank = tank;
        this.hull = hull;
        this.turrets = turrets;
        this.cannons = cannons;
        this.sniperCameraPos = SniperCameraPos.transform;
    }

    /// <summary>
    /// Assigns the player's camera and locates debug aim object if available.
    /// </summary>
    public void SetPlayerCamera(Camera camera)
    {
        playerCamera = camera;

        var da = GameObject.FindGameObjectWithTag("DEBUGAIM");
        if (da != null) 
        { 
            debugAimObject = da.transform;
            Debug.Log("Debug Aim Found!");
        }
        else
        {
            Debug.Log("Debug Aim NOT Found...");
        }
    }

    /// <summary>
    /// Immediately submits the given position and rotation to the server.
    /// </summary>
    public void ForceUpdateServerPos(Vector3 pos, Quaternion rot)
    {
        SubmitPositionToServerServerRpc(pos, rot);
    }

    /// <summary>
    /// Main update loop for owner player: handles movement, aiming, syncing, etc.
    /// </summary>
    void Update()
    {
        if (CanMove() == false) return;

        if (hull == null || turrets == null || cannons == null || sniperCameraPos == null)
        {
            if (currentTank != null) FixTankRunTime();
            return;
        }

        if (IsOwner)
        {
            HandleHullMovement();
            SniperCheck();
            UpdateAimPoint();
            HandleTurretRotation();
            HandleCannonElevation();
            SyncTurretAndCannonRotations(); // from player to server/other players
            RightSideUp();
        }
    }

    /// <summary>
    /// Flips the tank upright if it's upside down and player presses the key.
    /// </summary>
    private void RightSideUp()
    {

        if (Vector3.Dot(transform.up, Vector3.down) > 0.5f)
        {
            // prompt user to flip the tank
            HUDUI.Singleton.FlipPromptActive(true);

            // check for key down input
            if (Input.GetKeyDown(Settings.Singleton.KeyCodeFromSetting("Control-FlipTank")))
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Temporarily disable physics to avoid conflict
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;

                    rb.MovePosition(transform.position + new Vector3(0, 5, 0));
                    Quaternion upright = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                    rb.MoveRotation(upright);
                }
                else
                {
                    // Fallback if no Rigidbody
                    transform.position += new Vector3(0, 5, 0);
                    transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                }
            }
        }
        else
        {
            HUDUI.Singleton.FlipPromptActive(false);
        }
    }

    /// <summary>
    /// Server-side movement validation and correction for non-owners.
    /// </summary>
    void LateUpdate()
    {
        if (IsServer && !IsOwner)
        {
            ServerValidateClientMovement();
            lastServerPosition = transform.position;
        }
    }

    /// <summary>
    /// Reinitializes tank visuals and components at runtime if needed.
    /// </summary>
    void FixTankRunTime()
    {
        GetComponent<Player>().ChangeTank(GetComponent<Player>().TankVarient);
        GetComponent<TankVisuals>().RefreshList();
    }

    /// <summary>
    /// Handles hull movement and rotation based on input.
    /// </summary>
    void HandleHullMovement()
    {
        float moveInput = 0;
        float rotateInput = 0;
        if (Input.GetKey(Settings.Singleton.KeyCodeFromSetting("Control-Left")))
        {
            rotateInput -= 1;
        }
        if (Input.GetKey(Settings.Singleton.KeyCodeFromSetting("Control-Right")))
        {
            rotateInput += 1;
        }
        if (Input.GetKey(Settings.Singleton.KeyCodeFromSetting("Control-Forward")))
        {
            moveInput += 1;
        }
        if (Input.GetKey(Settings.Singleton.KeyCodeFromSetting("Control-Back")))
        {
            moveInput -= 1;
        }

        if (moveInput < 0) rotateInput *= -1;

        if (GetComponent<TankCombat>().repairing) { return; } // cant move while repairing

        if (GetComponent<TankCombat>().canDrive.Value)
        {
            hull.transform.Translate(Vector3.forward * moveInput * moveSpeed * Time.deltaTime);
            hull.transform.Rotate(Vector3.up * rotateInput * rotationSpeed * Time.deltaTime);

            SubmitPositionToServerServerRpc(hull.transform.position, hull.transform.rotation);
        }

    }

    /// <summary>
    /// ServerRpc to receive position/rotation from client.
    /// </summary>
    [ServerRpc (RequireOwnership=false)]
    public void SubmitPositionToServerServerRpc(Vector3 position, Quaternion rotation)
    {
        lastServerPosition = position;
        lastServerRotation = rotation;
    }

    /// <summary>
    /// Forces client position reset when server detects invalid movement.
    /// </summary>
    [ClientRpc]
    void MovePlayerBackClientRpc(Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;
    }

    /// <summary>
    /// Validates movement on server and applies correction if threshold exceeded.
    /// </summary>
    void ServerValidateClientMovement()
    {
        if (hull == null) return;
        if (canMove.Value == false) return;   

        float distance = Vector3.Distance(transform.position, lastServerPosition);
        if (distance > correctionThreshold)
        {
            float x = Mathf.Clamp(transform.position.x, transform.position.x - correctionThreshold, transform.position.x + correctionThreshold);
            float y = Mathf.Clamp(transform.position.y, transform.position.y - correctionThreshold, transform.position.y + correctionThreshold);
            float z = Mathf.Clamp(transform.position.z, transform.position.z - correctionThreshold, transform.position.z + correctionThreshold);

            Vector3 clampedPosition = new Vector3(x, y, z);

            MovePlayerBackClientRpc(clampedPosition, lastServerRotation);
            lastServerPosition = clampedPosition;
            Debug.LogWarning($"Correcting client movement. Distance was {distance}.");
        }
    }

    /// <summary>
    /// Updates the aim point based on raycast from the player camera.
    /// </summary>
    void UpdateAimPoint()
    {
        if (playerCamera == null && IsOwner) playerCamera = Camera.main;
        if (playerCamera == null) return;
        if (Input.GetKeyDown(Settings.Singleton.KeyCodeFromSetting("Control-Freelook"))) { return; } // free look camera

        int layerMask = ~((1 << 2) | (1 << 3));
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

    /// <summary>
    /// Rotates all turrets to face the aim point.
    /// </summary>
    void HandleTurretRotation()
    {
        if (turrets == null) return;

        turretRotations.Clear();

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
            turretRotations.Add(turret.transform.localRotation);
        }
    }

    /// <summary>
    /// Elevates each cannon toward the aim point within angular limits.
    /// </summary>
    void HandleCannonElevation()
    {
        if (cannons == null) return;

        cannonRotations.Clear();

        foreach (GameObject cannon in cannons)
        {
            Transform cannonBarrel = cannon.transform.GetChild(0);
            Vector3 direction = aimPoint - cannonBarrel.position;
            Vector3 localDirection = cannonBarrel.parent.InverseTransformDirection(direction.normalized);

            float pitchAngle = -Mathf.Atan2(localDirection.y, new Vector2(localDirection.x, localDirection.z).magnitude) * Mathf.Rad2Deg;
            pitchAngle = Mathf.Clamp(pitchAngle, minCannonAngle, maxCannonAngle);

            Quaternion targetRotation = Quaternion.Euler(pitchAngle, 0, 0);
            cannonBarrel.localRotation = Quaternion.Lerp(cannonBarrel.localRotation, targetRotation, cannonElevationSpeed * Time.deltaTime);

            cannonRotations.Add(cannonBarrel.localRotation);

            if (debugAimObject != null)
            {
                Ray ray = new Ray(cannonBarrel.position, cannonBarrel.forward);
                int layerMask = ~((1 << 2) | (1 << 3));
                if (Physics.Raycast(ray, out RaycastHit currentHit, 1000f, layerMask))
                {
                    Debug.DrawLine(cannonBarrel.position, currentHit.point, Color.yellow);
                }
                else
                {
                    Vector3 currentAimPoint = cannonBarrel.position + cannonBarrel.forward * 1000f;
                    Debug.DrawLine(cannonBarrel.position, currentAimPoint, Color.yellow);
                }
            }
        }
    }

    /// <summary>
    /// Sends turret and cannon rotations to server for syncing.
    /// </summary>
    void SyncTurretAndCannonRotations()
    {
        if (!IsOwner) return;


        Vector3[] turretEuler = new Vector3[turretRotations.Count];
        Vector3[] cannonEuler = new Vector3[cannonRotations.Count];

        for (int i = 0; i < turretRotations.Count; i++)
            turretEuler[i] = turretRotations[i].eulerAngles;

        for (int i = 0; i < cannonRotations.Count; i++)
            cannonEuler[i] = cannonRotations[i].eulerAngles;

        SendRotationsToServerServerRpc(turretEuler, cannonEuler);
    }

    /// <summary>
    /// Sends current rotations to server for synchronization
    /// </summary>
    /// <param name="turretEuler">Turret rotation</param>
    /// <param name="cannonEuler">Cannon rotation</param>
    [ServerRpc]
    void SendRotationsToServerServerRpc(Vector3[] turretEuler, Vector3[] cannonEuler)
    {
        for (int i = 0; i < turrets.Count && i < turretEuler.Length; i++)
        {
            turrets[i].transform.localEulerAngles = new Vector3(0f, turretEuler[i].y, 0f);
        }

        for (int i = 0; i < cannons.Count && i < cannonEuler.Length; i++)
        {
            Transform cannonBarrel = cannons[i].transform.GetChild(0);
            cannonBarrel.localEulerAngles = cannonEuler[i];
        }

        SyncTurretAndCannonRotationsClientRpc(turretEuler, cannonEuler);
    }

    /// <summary>
    /// Recieves rotations from server for player synchronization
    /// </summary>
    /// <param name="syncedTurretEulerAngles">Turret rotation</param>
    /// <param name="syncedCannonEulerAngles">Cannon rotation</param>
    [ClientRpc]
    void SyncTurretAndCannonRotationsClientRpc(Vector3[] syncedTurretEulerAngles, Vector3[] syncedCannonEulerAngles)
    {
        if (IsOwner) return;

        for (int i = 0; i < turrets.Count && i < syncedTurretEulerAngles.Length; i++)
        {
            turrets[i].transform.localEulerAngles = new Vector3(0f, syncedTurretEulerAngles[i].y, 0f);
        }

        for (int i = 0; i < cannons.Count && i < syncedCannonEulerAngles.Length; i++)
        {
            Transform cannonBarrel = cannons[i].transform.GetChild(0);
            cannonBarrel.localEulerAngles = syncedCannonEulerAngles[i];
        }
    }

    /// <summary>
    /// Toggles sniper mode when the appropriate key is pressed.
    /// </summary>
    void SniperCheck()
    {
        if (Input.GetKeyDown(Settings.Singleton.KeyCodeFromSetting("Control-SniperMode"))) sniperMode = !sniperMode;
    }

    /// <summary>
    /// Returns a reference to the selected cannon GameObject.
    /// </summary>
    public GameObject GetCannon(int cannonIndex) => cannons[cannonIndex];

    /// <summary>
    /// Called when the object is spawned on the network.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        canMove.OnValueChanged += OnCanMoveChanged;
    }

    /// <summary>
    /// Called when the object is despawned from the network.
    /// </summary>
    public override void OnNetworkDespawn()
    {
        canMove.OnValueChanged -= OnCanMoveChanged;
    }

    /// <summary>
    /// Helper function to log if a player can move or not
    /// </summary>
    /// <param name="previousValue">Player previously could move?</param>
    /// <param name="newValue">Player currently can move?</param>
    private void OnCanMoveChanged(bool previousValue, bool newValue)
    {
        Debug.Log($"[Client {OwnerClientId}] CanMove changed: {previousValue} → {newValue}");
    }

    /// <summary>
    /// Requests the server to change this tank's CanMove flag.
    /// </summary>
    public void RequestCanMoveChange(bool newValue)
    {
        RequestCanMoveChangeServerRpc(newValue);
    }

    /// <summary>
    /// Server accepts requests to change canMove flag
    /// </summary>
    /// <param name="newValue">Player can move?</param>
    /// <param name="rpcParams">Netcode parametet values</param>
    [ServerRpc]
    private void RequestCanMoveChangeServerRpc(bool newValue, ServerRpcParams rpcParams = default)
    {
        SetCanMoveOnServer(newValue);
    }

    /// <summary>
    /// Called on server to update the CanMove flag for this tank.
    /// </summary>
    public void SetCanMoveOnServer(bool newValue)
    {
        canMove.Value = newValue;
        Debug.Log($"[Server] Set CanMove for client {OwnerClientId} to {newValue}");
    }
}
