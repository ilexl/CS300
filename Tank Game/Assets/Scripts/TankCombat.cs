using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

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
    public float MaxReload
    {
        get; private set;
    }
    public float CurrentReload
    {
        get; private set;
    }
    [SerializeField] float damage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MaxHealth = 100f; // TODO: determine max health for each tank
        MaxReload = 5f; // TODO: determine reload for each tank
        CurrentHealth = MaxHealth;
        
        UpdateHealthBar();
    }

    void Update()
    {
        if (GetComponent<Player>().LocalPlayer is false) { return; } // not controlling other players damage system...

        if (CurrentReload > 0f)
        {
            CurrentReload -= Time.deltaTime;
            if (CurrentReload < 0f)
            {
                CurrentReload = 0f;
            }
        }
        

        if (Input.GetMouseButtonDown(0))
        {
            // check reload state
            if (CurrentReload > 0f) 
            {
                Debug.Log($"Reloading... {CurrentReload}s remain!");
                return; // no shoot
            }
            CurrentReload = MaxReload;

            // raycast target
            RaycastHit hit;
            GameObject cannon = GetComponent<TankMovement>().GetCannon(0);
            int layerMask = ~(1 << 10); // hit everything but layer 10 (Layer 10 is the local player)
            if (Physics.Raycast(cannon.transform.position, cannon.transform.forward, out hit, 1000f, layerMask))
            {
                GameObject hitObject = hit.collider.gameObject;
                Debug.Log("Hit object: " + hitObject.name);
                // get object hit

                TankCombat tc = hitObject.GetComponentInParent<TankCombat>();
                if(tc is not null)
                {
                    Debug.Log(tc);
                    ShootTank(tc);
                    Debug.Log($"Player has shot {tc.gameObject.name}");
                }


                // TODO: allow other object to be shot at (destructable objects)
                Debug.Log("Shot");


            }
            else
            {
                Debug.Log("Shot Missed");
            }
        }
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
        target.ChangeHealth(-damage);
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(TankCombat))]
public class EDITOR_TANKCOMBAT : Editor
{
    bool refresh = false;
    private void UpdateEditor()
    {
        if (refresh)
        // Repaint the inspector for live updating
        Repaint();
    }
    private void OnDisable()
    {
        // Always remove the update callback when the editor is disabled
        EditorApplication.update -= UpdateEditor;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        TankCombat tc = (TankCombat)target;

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField("Current Health", $"{tc.CurrentHealth}"); // Looks exactly like a greyed-out field
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField("Current Reload", $"{tc.CurrentReload}"); // Looks exactly like a greyed-out field
        EditorGUI.EndDisabledGroup();

        // Create toggle with tooltip
        bool newRefresh = EditorGUILayout.Toggle(
            new GUIContent("Refresh", "Enable this to constantly update the inspector"),
            refresh);

        // If the toggle changed
        if (newRefresh != refresh)
        {
            refresh = newRefresh;

            // Subscribe or unsubscribe from EditorApplication.update
            if (refresh) { EditorApplication.update += UpdateEditor; }
            else { EditorApplication.update -= UpdateEditor; }
        }

        if (GUILayout.Button("Refresh"))
        {
            tc.DEBUG_UpdateHealthBar();
        }
    }
}

#endif