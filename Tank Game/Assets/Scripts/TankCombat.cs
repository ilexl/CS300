using UnityEngine;
using Unity.Netcode;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TankCombat : NetworkBehaviour
{
    public NetworkVariable<float> maxHealth = new(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> currentHealth = new(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<float> maxReload = new(5f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> currentReload = new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] float damage = 25f;

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

        if (Input.GetMouseButtonDown(0))
        {
            if (currentReload.Value > 0f)
            {
                Debug.Log($"Reloading... {currentReload.Value:F1}s remain.");
                return;
            }

            GameObject cannon = GetComponent<TankMovement>()?.GetCannon(0);
            if (cannon == null) return;


            int layerMask = ~((1 << 2) | (1 << 10));
            RaycastHit hit;

            if (Physics.Raycast(cannon.transform.GetChild(0).position, cannon.transform.GetChild(0).forward, out hit, 10000f, layerMask))
            {
                Debug.Log(hit.collider.name);
                Debug.DrawLine(cannon.transform.GetChild(0).position, cannon.transform.GetChild(0).position + (cannon.transform.GetChild(0).forward * 1000f), Color.magenta, 30f);
                NetworkObject targetNetObj = hit.collider.GetComponentInParent<NetworkObject>();
                ulong targetClientId = targetNetObj != null ? targetNetObj.OwnerClientId : 0;
                RequestShootServerRpc(targetClientId);
            }
            else
            {
                RequestShootServerRpc(0);
            }
        }
    }

    [ServerRpc]
    private void RequestShootServerRpc(ulong targetClientId, ServerRpcParams rpcParams = default)
    {
        ulong shooterClientId = rpcParams.Receive.SenderClientId;

        if (!NetworkManager.ConnectedClients.TryGetValue(shooterClientId, out var shooterClient)) return;

        TankCombat shooterCombat = shooterClient.PlayerObject.GetComponent<TankCombat>();
        if (shooterCombat == null || shooterCombat.currentReload.Value > 0f)
        {
            Debug.Log("Server rejected shot: still reloading or invalid shooter.");
            return;
        }

        shooterCombat.currentReload.Value = shooterCombat.maxReload.Value;

        if (targetClientId != 0 &&
            NetworkManager.ConnectedClients.TryGetValue(targetClientId, out var targetClient))
        {
            TankCombat targetCombat = targetClient.PlayerObject.GetComponent<TankCombat>();
            if (targetCombat != null)
            {
                targetCombat.ApplyDamage(damage);
            }
        }

        BroadcastShotClientRpc(shooterClientId, targetClientId);
    }

    [ClientRpc]
    private void BroadcastShotClientRpc(ulong shooterClientId, ulong targetClientId)
    {
        Debug.Log($"[Client] Player {shooterClientId} shot at {(targetClientId != 0 ? $"target {targetClientId}" : "nothing")}");

        // TODO: play firing animation, sound, or effects
    }

    public void ApplyDamage(float amount)
    {
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

        currentHealth.Value = maxHealth.Value;
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
