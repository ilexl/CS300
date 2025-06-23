using UnityEngine;
using Unity.Netcode;
using Ballistics;
using System;
using Ballistics.Database;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Manages tank combat functionality, including shooting, damage, repairs, health updates,
/// and networking behavior using Unity Netcode.
/// </summary>
public class TankCombat : NetworkBehaviour
{
    public NetworkVariable<float> maxHealth = new(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> currentHealth = new(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    int maxCrew = 100;

    public NetworkVariable<float> maxReload = new(5f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> currentReload = new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] List<FunctionalTankModule> tankModules = new List<FunctionalTankModule>();
    public NetworkVariable<bool> canShoot = new(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> canDrive = new(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    public float MaxHealth => maxHealth.Value;
    public float CurrentHealth => currentHealth.Value;
    public float MaxReload => maxReload.Value;
    public float CurrentReload => currentReload.Value;

    [SerializeField] float repairHoldTimer = 0f;
    float repairHoldMax = 1.5f;
    [SerializeField] float repairTimer = 0f;
    float repairMax = 15f;
    public bool repairing = false;
    bool repairKeyLetUp = true;

    /// <summary>
    /// Initializes health and reload state on the server and updates UI.
    /// </summary>
    void Start()
    {
        if (IsServer)
        {
            maxHealth.Value = 100f;
            currentHealth.Value = maxHealth.Value;

            maxReload.Value = 5f;
            currentReload.Value = 0f;
        }

        UpdateHealthBar();
        repairKeyLetUp = true;
    }

    /// <summary>
    /// Unity Netcode hook called when the object spawns on the network.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        maxHealth.Value = 100f;
        currentHealth.Value = 100f;
        currentHealth.OnValueChanged += OnHealthChanged;
        UpdateHealthBar(); // Show initial state

    }

    /// <summary>
    /// Unity Netcode hook called when the object despawns from the network.
    /// </summary>
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        currentHealth.OnValueChanged -= OnHealthChanged;
    }

    /// <summary>
    /// Handles health changes and checks for death on the server.
    /// </summary>
    private void OnHealthChanged(float previous, float current)
    {
        Debug.Log($"Health updated to {current}");
        if (IsServer)
        {
            if(current <= 0f)
            {
                Debug.Log("Killing Player!!");
                InformAllPlayersOfDeath();
            }
        }
        UpdateHealthBar();
    }

    /// <summary>
    /// Handles reload, shooting, and repair interactions every frame.
    /// </summary>
    void Update()
    {
        // Reload countdown (server only)
        if (IsServer && currentReload.Value > 0f)
        {
            currentReload.Value -= Time.deltaTime;
            if (currentReload.Value < 0f)
                currentReload.Value = 0f;
        }
        

        if (!IsOwner) return;

        Debug.Log($"Crew alive == {GetAliveCrew()}");

        HUDUI.Singleton.UpdateReloadTime(1 - (currentReload.Value / maxReload.Value));

        // Primary fire input
        if (Input.GetKeyDown(Settings.Singleton.KeyCodeFromSetting("Control-ShootPrimary")))
        {
            if (currentReload.Value > 0f)
            {
                Debug.Log($"Reloading... {currentReload.Value:F1}s remain.");
                return;
            }
            
            Shoot();
        }

        // Repair logic (hold and toggle)
        bool canRepair = canDrive.Value == false || canShoot.Value == false; // if anything damaged then canRepair
        if (canRepair)
        {
            if (Input.GetKeyUp(Settings.Singleton.KeyCodeFromSetting("Control-Repair")))
            {
                repairKeyLetUp = true;
            }
            if (Input.GetKey(Settings.Singleton.KeyCodeFromSetting("Control-Repair")) && repairKeyLetUp == true)
            {
                repairHoldTimer += Time.deltaTime;
                if (repairHoldTimer >= repairHoldMax) // 1.5 seconds hold down to switch repair mode
                {
                    repairing = !repairing; // switch it
                    repairHoldTimer = 0f;
                    repairKeyLetUp = false;

                    HUDUI.Singleton.ShowRepairUI(repairing ? 1 : 0);
                }
            }
            else if (repairing == false && repairHoldTimer > 0f)
            {
                repairHoldTimer -= Time.deltaTime;
                if (repairHoldTimer < 0f)
                {
                    repairHoldTimer = 0f;
                }
            }

            if (repairHoldTimer != 0f)
            {
                float progress = repairHoldTimer / repairHoldMax;
                HUDUI.Singleton.ShowRepairUI(repairing ? 1 - progress : progress);
            }


            if (repairing == true)
            {
                repairTimer += Time.deltaTime;
                if (repairTimer >= repairMax)
                {
                    repairTimer = 0f;
                    repairing = false;
                    RepairTank();
                    HUDUI.Singleton.ShowRepairUI(0);
                }
            }
            else if (repairing == false && repairTimer > 0f)
            {
                repairTimer = 0f;
            }
            HUDUI.Singleton.ShowRepairTimer(repairMax - repairTimer);
        }
        else
        {
            HUDUI.Singleton.ShowRepairUI(0); // hides the repair UI
        }

    }

    /// <summary>
    /// Fully repairs relevant modules and notifies the server.
    /// </summary>
    private void RepairTank()
    {
        foreach(FunctionalTankModule component in tankModules)
        {
            switch (component.CurrentType)
            {
                case FunctionalTankModule.Type.Engine:
                case FunctionalTankModule.Type.Transmission:
                case FunctionalTankModule.Type.Track:
                case FunctionalTankModule.Type.Wheel:
                case FunctionalTankModule.Type.Barrel:
                case FunctionalTankModule.Type.Breach:
                    {
                        component.Health = component.GetInitialHealth();
                        UpdateHealthServerRpc(tankModules.IndexOf(component), component.Health);
                    }
                    break;
                default:
                    break;
            }
            
        }
    }

    /// <summary>
    /// Client-side shoot request (validated on server).
    /// </summary>
    void Shoot()
    {
        if (canShoot.Value)
        {
            GameObject cannon = GetComponent<TankMovement>()?.GetCannon(0);
            if (cannon == null) return;

            RequestShootServerRpc(cannon.transform.GetChild(0).position + (cannon.transform.GetChild(0).forward * 10), cannon.transform.GetChild(0).forward, (int)ProjectileKey.T99APT);
        }
    }

    /// <summary>
    /// Server validates client shoot request
    /// </summary>
    /// <param name="pos">Projectile start position</param>
    /// <param name="dir">Projectile direction</param>
    /// <param name="projectile">Type of projectile</param>
    /// <param name="rpcParams">Netcode params</param>
    [ServerRpc]
    private void RequestShootServerRpc(Vector3 pos, Vector3 dir, int projectile, ServerRpcParams rpcParams = default)
    {
        ulong shooterClientId = rpcParams.Receive.SenderClientId;
        if (!NetworkManager.ConnectedClients.TryGetValue(shooterClientId, out var shooterClient)) return; // Ensure player is valid

        currentReload.Value = maxReload.Value;

        long ms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        BroadcastShotClientRpc(shooterClientId, ms, pos, dir, projectile);
    }

    /// <summary>
    /// Server sends to all players about a projectile
    /// </summary>
    /// <param name="shooterClientId">Player that shot the projectile</param>
    /// <param name="seed">RNG seed so collisions are the same for all players</param>
    /// <param name="pos">Projectile start position</param>
    /// <param name="dir">Projectile direction</param>
    /// <param name="projectile">Type of projectile</param>
    [ClientRpc]
    private void BroadcastShotClientRpc(ulong shooterClientId, long seed, Vector3 pos, Vector3 dir, int projectile)
    {
        ProjectileDefinition projectileDefinition = ProjectileDatabase.GetProjectile(ProjectileKey.T99APT);
        Projectile.Create(pos, dir, seed, projectileDefinition);
    }

    /// <summary>
    /// Server informs all players that a player has died
    /// </summary>
    /// <param name="deadClientId">Player that died</param>
    [ClientRpc]
    private void InformPlayersOfDeathClientRpc(ulong deadClientId)
    {
        Debug.Log($"Client told by server to kill player");
        Debug.Log($"[Client] Player {deadClientId} has died.");
        // TODO: Show killfeed, play sound, display explosion, etc.
        if (IsOwner)
        {
            HUDUI.Singleton.ShowTankSelectionUI();
        }
    }

    /// <summary>
    /// Server-side death event: notifies clients and reports to RespawnManager.
    /// </summary>
    private void InformAllPlayersOfDeath()
    {
        Debug.Log("All players being told to kill player");
        InformPlayersOfDeathClientRpc(OwnerClientId); // server -> clients
        RespawnManager.Singleton.ReportPlayerDeath(OwnerClientId);
        Debug.Log("All players have been told to kill player");
    }

    /// <summary>
    /// Updates the player's health bar on HUD and team overlay.
    /// </summary>
    public void UpdateHealthBar()
    {
        PlayerTeam pt = GetComponent<PlayerTeam>();
        pt?.UpdateHealthBar(currentHealth.Value, maxHealth.Value);

        if (GetComponent<Player>().LocalPlayer && HUDUI.Singleton != null)
        {
            HUDUI.Singleton.UpdateHealth(currentHealth.Value, maxHealth.Value);
        }
    }

    /// <summary>
    /// Initializes tank modules and updates UI/state based on crew types.
    /// </summary>
    public void Setup()
    {
        tankModules = new List<FunctionalTankModule>();
        tankModules = GetComponentsInChildren<FunctionalTankModule>().ToList();
        // get all FunctionalTankModule and types

        int _maxCrew = 0;
        foreach (var tankModule in tankModules)
        {
            Debug.Log($"{tankModule.gameObject} is of type {tankModule.CurrentType}");
            switch (tankModule.CurrentType)
            {
                case FunctionalTankModule.Type.Commander:
                case FunctionalTankModule.Type.Driver:
                case FunctionalTankModule.Type.Gunner:
                case FunctionalTankModule.Type.Loader:
                    {
                        _maxCrew++;
                    }
                    break;
                default:
                    break;
            }
        }
        if (HUDUI.Singleton != null)
        {
            HUDUI.Singleton.UpdateComponentsUI(tankModules);
        }
        if (IsServer)
        {
            maxCrew = _maxCrew;
            maxHealth.Value = 100f;
            currentHealth.Value = 100f;
            Debug.Log($"Player {GetComponent<NetworkObject>().OwnerClientId} has {maxCrew} crew members");
            canDrive.Value = true;
            canShoot.Value = true;
        }
    }

    /// <summary>
    /// Called when a module takes damage; checks for side effects.
    /// </summary>
    public void ComponentHealthUpdate(FunctionalTankModule component)
    {
        if (IsServer)
        {
            switch (component.CurrentType)
            {
                case FunctionalTankModule.Type.Engine:
                case FunctionalTankModule.Type.Transmission:
                    {
                        CheckCanDrive();
                    }
                    break;
                case FunctionalTankModule.Type.Barrel:
                case FunctionalTankModule.Type.Breach:
                    {
                        CheckCanShoot();
                    }
                    break;
                case FunctionalTankModule.Type.Ammo:
                    {
                        if (component.Health == 0)
                        {
                            currentHealth.Value -= 99999; // lethal explosion
                        }
                    }
                    break;
                case FunctionalTankModule.Type.Commander:
                case FunctionalTankModule.Type.Driver:
                case FunctionalTankModule.Type.Gunner:
                case FunctionalTankModule.Type.Loader:
                    {
                        int aliveCrew = GetAliveCrew();
                        currentHealth.Value = aliveCrew * (float)((float)100f / (float)maxCrew);
                    }
                    break;
                case FunctionalTankModule.Type.Wheel:
                case FunctionalTankModule.Type.Track:
                case FunctionalTankModule.Type.None:
                default:
                    {
                        Debug.Log("Module Type damaged not implemented...");
                    }
                    break;
            }

            if(currentHealth.Value <= 0)
            {
                InformAllPlayersOfDeath();
            }
        }
        else if(IsOwner == false)
        {
            if (HUDUI.Singleton != null)
            {
                HUDUI.Singleton.UpdateComponentsUI(tankModules);
            }

            UpdateHealthServerRpc(tankModules.IndexOf(component), component.Health);
        }
    }

    /// <summary>
    /// Updates the health of a tank module on the server and synchronizes it to clients.
    /// </summary>
    /// <param name="index">Index of the module in the tankModules list.</param>
    /// <param name="newHealth">New health value to assign.</param>
    [ServerRpc(RequireOwnership = false)]
    void UpdateHealthServerRpc(int index, float newHealth)
    {
        FunctionalTankModule tankModule = tankModules[index];
        tankModule.Health = newHealth;
        UpdateHealthClientRpc(index, newHealth);
        CheckCanDrive();
        CheckCanShoot();
    }

    /// <summary>
    /// Evaluates whether the tank is drivable based on engine and transmission health.
    /// Updates the canDrive network variable.
    /// </summary>
    void CheckCanDrive()
    {
        bool _canDrive = true; // assume both working
        foreach (var _tankModule in tankModules)
        {
            // if either dont work - return cannot drive
            if (_tankModule.CurrentType == FunctionalTankModule.Type.Engine && _tankModule.Health <= 0) { _canDrive = false; }
            if (_tankModule.CurrentType == FunctionalTankModule.Type.Transmission && _tankModule.Health <= 0) { _canDrive = false; }
        }
        canDrive.Value = _canDrive;
        Debug.Log($"Can drive set to {_canDrive}");
    }

    /// <summary>
    /// Evaluates whether the tank can fire based on barrel and breach health.
    /// Updates the canShoot network variable.
    /// </summary>
    void CheckCanShoot()
    {
        bool _canShoot = true; // assume both working
        foreach (var tankModule in tankModules)
        {
            // if either dont work - return cannot shoot
            if (tankModule.CurrentType == FunctionalTankModule.Type.Barrel && tankModule.Health <= 0) { _canShoot = false; }
            if (tankModule.CurrentType == FunctionalTankModule.Type.Breach && tankModule.Health <= 0) { _canShoot = false; }
        }
        canShoot.Value = _canShoot;
        Debug.Log($"Can shoot set to {_canShoot}");
    }

    /// <summary>
    /// Called by the server to update a module's health on all clients.
    /// Also triggers UI updates if the local player owns the tank.
    /// </summary>
    /// <param name="index">Index of the module in the list.</param>
    /// <param name="newHealth">New health value to apply.</param>
    [ClientRpc]
    void UpdateHealthClientRpc(int index, float newHealth)
    {
        FunctionalTankModule tankModule = tankModules[index];
        tankModule.ServerSetHealth(newHealth);
        if (IsOwner)
        {
            if (HUDUI.Singleton != null)
            {
                HUDUI.Singleton.UpdateComponentsUI(tankModules);
            }
        }
    }

    /// <summary>
    /// Calculates the number of crew modules with health greater than zero.
    /// Used to determine percentage-based health from crew survival.
    /// </summary>
    /// <returns>The number of alive crew members.</returns>
    private int GetAliveCrew()
    {
        int aliveCrew = 0;
        foreach (var tankModule in tankModules)
        {
            switch (tankModule.CurrentType)
            {
                case FunctionalTankModule.Type.Commander:
                case FunctionalTankModule.Type.Driver:
                case FunctionalTankModule.Type.Gunner:
                case FunctionalTankModule.Type.Loader:
                    {
                        if(tankModule.Health > 0)
                        {
                            aliveCrew++;
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        return aliveCrew;
    }



    /// <summary>
    /// Resets the player's health on the server.
    /// Can only be called from the server.
    /// </summary>
    public void ResetHealth()
    {
        if (!IsServer)
        {
            Debug.LogWarning("ResetHealth was called from a non-server instance.");
            return;
        }

        Debug.Log("Health Reset");

        currentHealth.Value = 100;
        UpdateHealthBar();
    }

#if UNITY_EDITOR
    public void DEBUG_UpdateHealthBar() => UpdateHealthBar();
#endif
}


#if UNITY_EDITOR
/// <summary>
/// Custom editor for TankCombat for live debugging in the Unity Editor.
/// </summary>
[CustomEditor(typeof(TankCombat))]
public class EDITOR_TANKCOMBAT : Editor
{
    bool refresh = false;
    private void UpdateEditor()
    {
        if (refresh)
            Repaint();
    }

    private void OnDisable()
    {
        EditorApplication.update -= UpdateEditor;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TankCombat tc = (TankCombat)target;

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField("Current Health", $"{tc.CurrentHealth}");
        EditorGUILayout.TextField("Current Reload", $"{tc.CurrentReload}");
        EditorGUI.EndDisabledGroup();

        bool newRefresh = EditorGUILayout.Toggle(new GUIContent("Refresh", "Auto-refresh inspector values"), refresh);
        if (newRefresh != refresh)
        {
            refresh = newRefresh;
            if (refresh) EditorApplication.update += UpdateEditor;
            else EditorApplication.update -= UpdateEditor;
        }

        if (GUILayout.Button("Refresh"))
        {
            tc.DEBUG_UpdateHealthBar();
        }
    }
}
#endif
