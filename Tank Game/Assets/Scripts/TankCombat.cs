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

        //if (Input.GetMouseButtonDown(0))
        if(Input.GetKeyDown(Settings.Singleton.KeyCodeFromSetting("Control-ShootPrimary")))
        {
            if (currentReload.Value > 0f)
            {
                Debug.Log($"Reloading... {currentReload.Value:F1}s remain.");
                return;
            }
            
            Shoot();
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

        // do server stuff here
        long ms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // server also creates this and simulates
        ProjectileDefinition projectileDefinition = ProjectileDatabase.GetProjectile(ProjectileKey.T99APT);
        Projectile.Create(pos, dir, ms, projectileDefinition);

        // tell all the players
        BroadcastShotClientRpc(shooterClientId, ms, pos, dir, projectile);
    }

    [ClientRpc]
    private void BroadcastShotClientRpc(ulong shooterClientId, long seed, Vector3 pos, Vector3 dir, int projectile)
    {
        ProjectileDefinition projectileDefinition = ProjectileDatabase.GetProjectile(ProjectileKey.T99APT);
        Projectile.Create(pos, dir, seed, projectileDefinition);
    }

    public void ApplyDamage(float amount)
    {
        if(!IsServer) return; // only the server will run this code

        currentHealth.Value -= amount;
        if (currentHealth.Value < 0f)
            currentHealth.Value = 0f;

        UpdateHealthBar();

        if(currentHealth.Value <= 0f)
        {
            PlayerDeath(); // destroy tank if health is 0
        }
    }

    private void PlayerDeath()
    {
        Debug.Log($"Player {OwnerClientId} died");
        InformAllPlayersOfDeath();
    }

    [ClientRpc]
    private void InformPlayersOfDeathClientRpc(ulong deadClientId)
    {
        Debug.Log($"[Client] Player {deadClientId} has died.");
        // TODO: Show killfeed, play sound, display explosion, etc.
        if (IsOwner)
        {
            RespawnManager.Singleton.ReportPlayerDeathServerRpc();
        }
    }

    private void InformAllPlayersOfDeath()
    {
        InformPlayersOfDeathClientRpc(OwnerClientId); // server -> clients
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
        }
    }

    public void ComponentHealthUpdate(FunctionalTankModule component)
    {
        Debug.Log("ComponentHealthUpdate called for {component.gameObject}");
        switch (component.CurrentType)
        {
            case FunctionalTankModule.Type.Engine:
            case FunctionalTankModule.Type.Transmission:
                if (IsServer)
                {
                    bool _canDrive = true; // assume both working
                    foreach(var tankModule in tankModules)
                    {
                        // if either dont work - return cannot drive
                        if (tankModule.CurrentType == FunctionalTankModule.Type.Engine && tankModule.Health == 0) { _canDrive = false; }
                        if (tankModule.CurrentType == FunctionalTankModule.Type.Transmission && tankModule.Health == 0) { _canDrive = false; }
                    }
                    canDrive.Value = _canDrive; 
                }
                break;
            case FunctionalTankModule.Type.Barrel:
            case FunctionalTankModule.Type.Breach:
                if (IsServer)
                {
                    bool _canShoot = true; // assume both working
                    foreach (var tankModule in tankModules)
                    {
                        // if either dont work - return cannot shoot
                        if (tankModule.CurrentType == FunctionalTankModule.Type.Engine && tankModule.Health == 0) { _canShoot = false; }
                        if (tankModule.CurrentType == FunctionalTankModule.Type.Transmission && tankModule.Health == 0) { _canShoot = false; }
                    }
                    canShoot.Value = _canShoot;
                }
                break;
            case FunctionalTankModule.Type.Ammo:
                if (IsServer)
                {
                    if(component.Health == 0)
                    {
                        ApplyDamage(99999); // force kill player
                    }
                }
                break;
            case FunctionalTankModule.Type.Commander:
            case FunctionalTankModule.Type.Driver:
            case FunctionalTankModule.Type.Gunner:
            case FunctionalTankModule.Type.Loader:
                if (IsServer)
                {
                    int aliveCrew = GetAliveCrew();
                    currentHealth.Value = aliveCrew * (maxCrew / 100);
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

        // in all cases the entire UI should be refreshed...
        if(HUDUI.Singleton != null)
        {
            HUDUI.Singleton.UpdateComponentsUI(tankModules);
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
                        if(tankModule.Health != 0)
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
