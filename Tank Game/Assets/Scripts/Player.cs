using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(TankMovement))]
[RequireComponent(typeof(TankVisuals))]
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
            Debug.LogWarning("Cannot change tank to null");
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
        if (tank.hullModel == null)
        {
            Debug.LogError($"{tank.tankName} [hull model] == null... STOPING EXECUTION");
            return;
        }
        GameObject hull = Instantiate(tank.hullModel, this.transform);
        if (tank.hullPosition == null)
        {
            Debug.LogError($"{tank.tankName} [hull position] == null... STOPING EXECUTION");
            return;
        }
        hull.transform.localPosition = tank.hullPosition;

        // turrets
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
            GameObject tm = tank.turretModels[i]; // no check as for loop has max defined
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
            turrets.Add(holder);
        }

        // cannons
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
            GameObject cm = tank.cannonModels[i]; // no check as for loop has max defined
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
            try { c.transform.localEulerAngles = tank.cannonRotations[i]; }
            #region catch
            catch (IndexOutOfRangeException)
            {
                Debug.LogError($"{tank.tankName} [cannon rotations] @ index {i} == OUT OF RANGE... SET TO 0 and CONTINUING");
                c.transform.localEulerAngles = Vector3.zero;
            }
            #endregion
            cannons.Add(holder);
        }

        // update overall scale
        hull.transform.localScale = tank.modelScale;

        // create empty gameobject for camera pos in snipermode
        GameObject sniperPos = Instantiate(holderPrefab, hull.transform);
        sniperPos.transform.localPosition = tank.cameraPosSniper;
        sniperPos.name = "SNIPERMODE CAMERA POSITION";

        // update tank movement script
        tankMovement.UpdateTank(tank, gameObject, turrets, cannons, sniperPos);
        Debug.Log("TankMovement Script Updated!");

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
