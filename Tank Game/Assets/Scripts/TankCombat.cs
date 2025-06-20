using UnityEngine;
using Unity.Netcode;
using Ballistics;
using System;
using Ballistics.Database;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;


#if UNITY_EDITOR
using UnityEditor;
#endif

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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        maxHealth.Value = 100f;
        currentHealth.Value = 100f;
        currentHealth.OnValueChanged += OnHealthChanged;
        UpdateHealthBar(); // Show initial state

    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        currentHealth.OnValueChanged -= OnHealthChanged;
    }

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

    void Update()
    {
        if (IsServer && currentReload.Value > 0f)
        {
            currentReload.Value -= Time.deltaTime;
            if (currentReload.Value < 0f)
                currentReload.Value = 0f;
        }
        

        if (!IsOwner) return;

        Debug.Log($"Crew alive == {GetAliveCrew()}");

        HUDUI.Singleton.UpdateReloadTime(1 - (currentReload.Value / maxReload.Value));

        //if (Input.GetMouseButtonDown(0))
        if (Input.GetKeyDown(Settings.Singleton.KeyCodeFromSetting("Control-ShootPrimary")))
        {
            if (currentReload.Value > 0f)
            {
                Debug.Log($"Reloading... {currentReload.Value:F1}s remain.");
                return;
            }
            
            Shoot();
        }

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

    void Shoot()
    {
        if (canShoot.Value)
        {
            GameObject cannon = GetComponent<TankMovement>()?.GetCannon(0);
            if (cannon == null) return;

            RequestShootServerRpc(cannon.transform.GetChild(0).position + (cannon.transform.GetChild(0).forward * 10), cannon.transform.GetChild(0).forward, (int)ProjectileKey.T99APT);
        }
    }

    [ServerRpc]
    private void RequestShootServerRpc(Vector3 pos, Vector3 dir, int projectile, ServerRpcParams rpcParams = default)
    {
        ulong shooterClientId = rpcParams.Receive.SenderClientId;
        if (!NetworkManager.ConnectedClients.TryGetValue(shooterClientId, out var shooterClient)) return; // ensure player is valid

        currentReload.Value = maxReload.Value;

        // do server stuff here
        long ms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // tell all the players
        BroadcastShotClientRpc(shooterClientId, ms, pos, dir, projectile);
    }

    [ClientRpc]
    private void BroadcastShotClientRpc(ulong shooterClientId, long seed, Vector3 pos, Vector3 dir, int projectile)
    {
        ProjectileDefinition projectileDefinition = ProjectileDatabase.GetProjectile(ProjectileKey.T99APT);
        Projectile.Create(pos, dir, seed, projectileDefinition);
    }


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

    private void InformAllPlayersOfDeath()
    {
        Debug.Log("All players being told to kill player");
        InformPlayersOfDeathClientRpc(OwnerClientId); // server -> clients
        RespawnManager.Singleton.ReportPlayerDeath(OwnerClientId);
        Debug.Log("All players have been told to kill player");
    }


    public void UpdateHealthBar()
    {
        PlayerTeam pt = GetComponent<PlayerTeam>();
        pt?.UpdateHealthBar(currentHealth.Value, maxHealth.Value);

        if (GetComponent<Player>().LocalPlayer && HUDUI.Singleton != null)
        {
            HUDUI.Singleton.UpdateHealth(currentHealth.Value, maxHealth.Value);
        }
    }

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

    public void ComponentHealthUpdate(FunctionalTankModule component)
    {
        //Debug.Log($"ComponentHealthUpdate called for {component.gameObject}");
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
                            currentHealth.Value -= 99999; // force kill player
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
        else if(IsOwner == false) // we should not apply own health - other player will tell the server what was hit
        {
            // in all cases the entire UI should be refreshed...
            if (HUDUI.Singleton != null)
            {
                HUDUI.Singleton.UpdateComponentsUI(tankModules);
            }

            UpdateHealthServerRpc(tankModules.IndexOf(component), component.Health);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateHealthServerRpc(int index, float newHealth)
    {
        FunctionalTankModule tankModule = tankModules[index];
        tankModule.Health = newHealth;
        UpdateHealthClientRpc(index, newHealth);
        CheckCanDrive();
        CheckCanShoot();
    }

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
