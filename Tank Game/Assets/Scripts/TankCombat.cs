using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TankCombat : MonoBehaviour
{
    public float MaxHealth
    {
        get; private set;
    }
    public float CurrentHealth
    {
        get; private set;
    }
    [SerializeField] float damage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MaxHealth = 100f; // TODO: determine max health for each tank
        CurrentHealth = MaxHealth;
        
        UpdateHealthBar();
    }

    void Update()
    {
            
    }

    void UpdateHealthBar()
    {
        PlayerTeam pt = GetComponent<PlayerTeam>();
        pt.UpdateHealthBar(CurrentHealth, MaxHealth);
    }

#if UNITY_EDITOR
    public void DEBUG_UpdateHealthBar()
    {
        UpdateHealthBar();
    }
#endif

    public void ChangeHealth(float change)
    {
        CurrentHealth += change;
        UpdateHealthBar();
    }

    public void SetMaxHealth(float newMax)
    {
        MaxHealth = newMax;
        UpdateHealthBar();
    }

    void ShootTank(TankCombat target)
    {

    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(TankCombat))]
public class EDITOR_TANKCOMBAT : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        TankCombat tc = (TankCombat)target;

        if (GUILayout.Button("Refresh"))
        {
            tc.DEBUG_UpdateHealthBar();
        }
    }
}

#endif