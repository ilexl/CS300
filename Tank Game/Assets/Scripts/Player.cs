using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;
using Unity.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Networked player class handling tank spawning, changing tanks, and player ownership.
/// Manages tank visuals, movement, and synchronizes tank variant selection across clients.
/// </summary>
public class Player : NetworkBehaviour
{
    [SerializeField] TankMovement tankMovement;
    [SerializeField] TankVisuals tankVisuals;
    [SerializeField] PlayerTeam playerTeam;
    [SerializeField] TankVarients currentTank;
    [SerializeField] GameObject holderPrefab;
    public bool LocalPlayer = false;

    // NetworkVariable to synchronize the tank variant's name between server and clients
    private NetworkVariable<FixedString64Bytes> currentTankName = new NetworkVariable<FixedString64Bytes>(string.Empty, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    /// <summary>
    /// Property wrapper for currentTank that triggers ChangeTank on set.
    /// </summary>
    public TankVarients TankVarient
    {
        get { return currentTank; }
        set
        {
            currentTank = value;
            ChangeTank(value);
        }
    }

    /// <summary>
    /// Called when the network object is spawned.
    /// Synchronizes tank variant and initializes local player setup.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // If a tank name has been synced previously, restore that tank variant
        if (!string.IsNullOrEmpty(currentTankName.Value.ToString()))
        {
            ChangeTank(currentTankName.Value.ToString());
        }
        else
        {
            // No tank selected, set to null
            ChangeTank((TankVarients)null);
        }

        if (IsOwner)
        {
            // Reset local player position and force server position update on spawn
            GetComponent<TankMovement>().ForceUpdateServerPos(new Vector3(0, 0, 0), Quaternion.identity);
            transform.position = new Vector3(0, 0, 0);
        }

        // Enable gravity for the rigidbody (tank physics)
        GetComponent<Rigidbody>().useGravity = true;
    }

    /// <summary>
    /// Editor-only validation method, called on changes in inspector.
    /// Cleans up child objects and reapplies current tank to avoid duplication.
    /// </summary>
    public void OnValidate()
    {
        #if UNITY_EDITOR
        // Only run if not playing or building
        if (EditorApplication.isPlaying is false)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorApplication.isUpdating) return; // Prevents execution during asset imports
            if (BuildPipeline.isBuildingPlayer) return; // Prevents issues during builds
        }

        // Use delayed call to safely destroy children after inspector updates
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null) return; // Object might be destroyed

            // Destroy all child gameobjects twice to ensure cleanup
            foreach (Transform child in transform)
            {
                DestroyImmediate(child.gameObject, true);
            }
            foreach (Transform child in transform)
            {
                DestroyImmediate(child.gameObject, true);
            }

            // Re-apply current tank visuals
            if (currentTank != null)
            {
                ChangeTank(currentTank);
            }
        };
        #endif
    }

    /// <summary>
    /// Initialization on game start.
    /// Disables gravity initially and assigns camera to local player tank.
    /// </summary>
    private void Start()
    {
        // Disable gravity until tank is set up
        GetComponent<Rigidbody>().useGravity = false;

        // Detect if this player instance is the local owner
        LocalPlayer = GetComponent<NetworkObject>().IsOwner;

        if(LocalPlayer)
        {
            // Assign main camera to follow this player’s tank
            Camera.main.GetComponent<CameraControl>().target = transform;
        }
        if (LocalPlayer) 
        {
            // Reset position and force server position update for local player
            GetComponent<TankMovement>().ForceUpdateServerPos(new Vector3(0, 0, 0), Quaternion.identity);
            transform.position = new Vector3(0, 0, 0);
        }
    }

    /// <summary>
    /// Changes tank variant by tank name string.
    /// Updates network variable and calls variant change method.
    /// </summary>
    public void ChangeTank(string tankName)
    {
        if (IsOwner)
        {
            // Sync the new tank name with network variable
            currentTankName.Value = tankName ?? string.Empty;
        }
        
        if (tankName == null) // return null tank if string doesnt exist
        {
            // Null input resets the tank variant to null
            ChangeTank((TankVarients)null);
        }

        // Attempt to find TankVarients object by name
        TankVarients tv = TankVarients.GetFromString(tankName);
        
        if (tv != null) 
        {
            // Valid tank variant found, apply it
            ChangeTank(tv);
            return;
        }

        // Tank name invalid, set tank to null
        ChangeTank((TankVarients)null);
    }

    /// <summary>
    /// Changes tank variant to the specified TankVarients object.
    /// Destroys previous tank models, instantiates new hull, turrets, cannons,
    /// updates movement and visuals, and handles colliders and physics.
    /// </summary>
    public void ChangeTank(TankVarients tank)
    {
        currentTank = tank;

        // Destroy all children (previous tank models)
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        if (IsOwner)
        {
            // Update network variable with new tank name or clear if null
            currentTankName.Value = tank == null ? string.Empty : tank.tankName;
        }

        // Disable collider and gravity while not a tank
        GetComponent<BoxCollider>().enabled = false;
        GetComponent<Rigidbody>().useGravity = false;


        if (tank == null)
        {
            // If no tank assigned, show respawn UI for local player or client (not server
            if (IsOwner) { return; }
            if (LocalPlayer == false) { return; }  
            if (HUDUI.Singleton != null)
            {
                Debug.Log("Showing Respawn Window as tank cannot be null...");
                HUDUI.Singleton.ShowRespawnUI();
            }
            return;
        }

        Debug.Log($"Tank changing to {tank.name}");

        // Ensure tankMovement script is assigned
        if(tankMovement == null)
        {
            Debug.LogError("Missing TankMovement (required script for player)");
            Debug.LogWarning($"Failed to change tank to {tank.name}");
            return;
        }


        // Validate hull model and instantiate it
        if (tank.hullModel == null)
        {
            Debug.LogError($"{tank.tankName} [hull model] == null... STOPING EXECUTION");
            return;
        }

        GameObject hull = Instantiate(tank.hullModel, this.transform);

        // Validate hull position and set it
        if (tank.hullPosition == null)
        {
            Debug.LogError($"{tank.tankName} [hull position] == null... STOPING EXECUTION");
            return;
        }
        hull.transform.localPosition = tank.hullPosition;
        hull.transform.localRotation = Quaternion.Euler(tank.hullRotation);

        // Update collider size to match hull dimensions
        GetComponent<BoxCollider>().size = new Vector3(tank.hullWidth, tank.hullHeight, tank.hullLength);

        // Instantiate turrets, parenting them under hull, with error checking for arrays
        List<GameObject> turrets = new List<GameObject>();
        for (int i = 0; i < tank.turretModels.Length; i++) 
        {
            GameObject holder = Instantiate(holderPrefab, hull.transform);
            try { holder.transform.localPosition = tank.turretPivotPoints[i]; } 
            #region catch
            catch (IndexOutOfRangeException)
            {
                Debug.LogError($"{tank.tankName} [turret pivot points] @ index {i} == OUT OF RANGE... SET TO 0 and CONTINUING");
                holder.transform.localPosition = Vector3.zero;
            }
            #endregion
            if (tank.hullModel == null)
            {
                Debug.LogError($"{tank.tankName} [hull model] == null... STOPING EXECUTION");
                return;
            }
            GameObject tm = tank.turretModels[i];
            if(tm == null)
            {
                Debug.LogError($"{tank.tankName} [turret model] @ index {i} == null... STOPING EXECUTION");
                return;
            }
            GameObject t = Instantiate(tm, holder.transform);
            try { t.transform.localPosition = tank.turretPositions[i] - tank.turretPivotPoints[i]; }
            #region catch
            catch (IndexOutOfRangeException)
            {
                Debug.LogError($"{tank.tankName} [turret positions OR turret pivot points] @ index {i} == OUT OF RANGE... SET TO 0 and CONTINUING");
                t.transform.localPosition = Vector3.zero;
            }
            #endregion
            try { t.transform.localEulerAngles = tank.turretRotations[i]; }
            #region catch
            catch (IndexOutOfRangeException)
            {
                Debug.LogError($"{tank.tankName} [turret rotations] @ index {i} == OUT OF RANGE... SET TO 0 and CONTINUING");
                t.transform.localEulerAngles = Vector3.zero;
            }
            #endregion
            holder.transform.localRotation = Quaternion.Euler(tank.turretRotations[i]);
            turrets.Add(holder);
        }

        // Instantiate cannons attached to turrets with error handling for arrays
        List<GameObject> cannons = new List<GameObject>();
        for (int i = 0; i < tank.cannonModels.Length; i++)
        {
            int turretIndex = 0;
            try { turretIndex = tank.cannonAttachedToTurretIndexs[i]; }
            #region catch
            catch (IndexOutOfRangeException)
            {
                Debug.LogError($"{tank.tankName} [cannon attached to turret indexs] @ index {i} == OUT OF RANGE... SET TO 0 and CONTINUING");
                turretIndex = 0;
            }
            #endregion
            GameObject holder = Instantiate(holderPrefab, turrets[turretIndex].transform);
            
            try { holder.transform.localPosition = tank.cannonPivotPoints[i]; }
            #region catch
            catch (IndexOutOfRangeException)
            {
                Debug.LogError($"{tank.tankName} [cannon pivot points] @ index {i} == OUT OF RANGE... SET TO 0 and CONTINUING");
                holder.transform.localPosition = Vector3.zero;
            }
            #endregion
            GameObject cm = tank.cannonModels[i];
            if (cm == null)
            {
                Debug.LogError($"{tank.tankName} [cannon model] @ index {i} == null... STOPING EXECUTION");
                return;
            }
            GameObject c = Instantiate(cm, holder.transform);
            try { c.transform.localPosition = tank.cannonPositions[i]; }
            #region catch
            catch (IndexOutOfRangeException)
            {
                Debug.LogError($"{tank.tankName} [cannon positions] @ index {i} == OUT OF RANGE... SET TO 0 and CONTINUING");
                c.transform.localPosition = Vector3.zero;
            }
            #endregion
            try { c.transform.localRotation = Quaternion.Euler(tank.cannonRotations[i]); }
            #region catch
            catch (IndexOutOfRangeException)
            {
                Debug.LogError($"{tank.tankName} [cannon rotations] @ index {i} == OUT OF RANGE... SET TO 0 and CONTINUING");
                c.transform.localEulerAngles = Vector3.zero;
            }
            #endregion
            cannons.Add(holder);
        }

        // Set overall hull scale based on tank variant
        hull.transform.localScale = tank.modelScale;

        // Create a holder gameobject to mark sniper camera position on the hull
        GameObject sniperPos = Instantiate(holderPrefab, hull.transform);
        sniperPos.transform.localPosition = tank.cameraPosSniper;
        sniperPos.name = "SNIPERMODE CAMERA POSITION";

        // Update tank movement with newly instantiated hull, turrets, cannons, sniperPos
        if (tankMovement == null || tankVisuals == null)
        {
            Debug.LogWarning("Player missing movement or visual scripts. " +
                "This may be intended but stops some code frome executing");
        }
        else
        {
            tankMovement.UpdateTank(tank, gameObject, turrets, cannons, sniperPos);

            // Assign camera for the local player only
            if (NetworkManager.Singleton != null) {
                if (NetworkManager.Singleton.LocalClient.PlayerObject != null)
                {
                    if (NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>() == this)
                    {
                        // run on local player
                        tankMovement.SetPlayerCamera(Camera.main);
                    }
                }
            }
            
            Debug.Log("TankMovement Script Updated!");
        }

        // Setup tank combat if component exists
        if (GetComponent<TankCombat>() != null)
        {
            GetComponent<TankCombat>().Setup();
        }

        // Enable collider and physics gravity now that tank is ready
        GetComponent<BoxCollider>().enabled = true;
        GetComponent<Rigidbody>().useGravity = true;

        // Add minimap icon for this player's team if PlayerTeam component exists
        if (GetComponent<PlayerTeam>() != null)
        {
            GetComponent<PlayerTeam>().AddMinimapIcon();
        }

        Debug.Log($"Tank successfully changed to {tank.name}");
    }
}

#if UNITY_EDITOR
/// <summary>
/// // Editor button for manually refreshing tank setup in inspector
/// </summary>
[CustomEditor(typeof(Player))]
public class EDITOR_Player : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Player p = (Player)target;

        if (GUILayout.Button("Refresh"))
        {
            p.OnValidate();
        }
    }
}

#endif
