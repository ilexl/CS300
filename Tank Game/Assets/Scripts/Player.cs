using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using log4net.Filter;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(TankMovement))]
public class Player : NetworkBehaviour
{
    [SerializeField] TankMovement tankMovement;
    [SerializeField] TankVarients currentTank;
    [SerializeField] GameObject holderPrefab;
    public TankVarients TankVarient
    {
        get { return currentTank; }
        set
        {
            currentTank = value;
            ChangeTank(value);
        }
    }

    public void OnValidate()
    { 
        #if UNITY_EDITOR
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (EditorApplication.isUpdating) return; // Prevents execution during asset imports
        if (BuildPipeline.isBuildingPlayer) return; // Prevents issues during builds

        // Schedule object destruction to avoid Unity serialization issues
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null) return; // Prevent null reference errors if the object was deleted

            foreach (Transform child in transform)
            {
                DestroyImmediate(child.gameObject, true);
            }
            foreach (Transform child in transform)
            {
                DestroyImmediate(child.gameObject, true);
            } 

            if (currentTank != null)
            {
                ChangeTank(currentTank);
            }
        };
        #endif
    }

    public void ChangeTank(TankVarients tank)
    {
        if (tank == null)
        {
            Debug.LogError("Cannot change tank to null");
            return;
        }

        Debug.Log($"Tank changing to {tank.name}");
        if(tankMovement == null)
        {
            Debug.LogError("Missing TankMovement (required script for player)");
            Debug.LogWarning($"Failed to change tank to {tank.name}");
            return;
        }
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // hull
        GameObject hull = Instantiate(tank.hullModel, this.transform);
        hull.transform.localPosition = tank.hullPosition;

        // turrets
        List<GameObject> turrets = new List<GameObject>();
        for (int i = 0; i < tank.turretModels.Length; i++) 
        {
            GameObject holder = Instantiate(holderPrefab, hull.transform);
            holder.transform.localPosition = tank.turretPivotPoints[i];
            GameObject tm = tank.turretModels[i];
            GameObject t = Instantiate(tm, holder.transform);
            t.transform.localPosition = tank.turretPositions[i] - tank.turretPivotPoints[i];
            t.transform.localEulerAngles = tank.turretRotations[i];
            turrets.Add(holder);
        }

        // cannons
        List<GameObject> cannons = new List<GameObject>();
        for (int i = 0; i < tank.cannonModels.Length; i++)
        {
            int turretIndex = tank.cannonAttachedToTurretIndexs[i];
            GameObject holder = Instantiate(holderPrefab, turrets[turretIndex].transform);
            holder.transform.localPosition = tank.cannonPivotPoints[i];
            GameObject cm = tank.cannonModels[i];
            Debug.Log(turretIndex);
            GameObject c = Instantiate(cm, holder.transform);
            c.transform.localPosition = tank.cannonPositions[i];
            c.transform.localEulerAngles = tank.cannonRotations[i];
            cannons.Add(holder);
        }

        // update overall scale
        hull.transform.localScale = tank.modelScale;

        // update tank movement script
        tankMovement.UpdateTank(tank, gameObject, turrets, cannons); 


        currentTank = tank;
        Debug.Log($"Tank successfully changed to {tank.name}");
    }
}

#if UNITY_EDITOR

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
