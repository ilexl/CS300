using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages tank selection logic across both the main menu and in-game context.
/// Handles slot initialization, UI updates, and integration with the RespawnManager.
/// </summary>
public class TankSelection : MonoBehaviour
{
    public static TankSelection Singleton;
    [SerializeField][Range(1f, 20f)] int unlockedSlots;
    [SerializeField] List<RectTransform> TankList;
    [SerializeField] GameObject holderPrefab;
    [SerializeField] Button battleBtn;

    /// <summary>
    /// Initializes singleton, recalculates layout widths, and populates tank data from saved preferences.
    /// </summary>
    void Start()
    {
        Singleton = this;
        RecalculateTankListWidth();
        SavedPrefsToCards();
    }

    /// <summary>
    /// Loads saved preferences into each tank card and enables/disables battle button based on slot contents.
    /// </summary>
    public void SavedPrefsToCards()
    {
        foreach (RectTransform transform in TankList)
        {
            foreach (Transform child in transform)
            {
                child.GetComponent<TankCard>().LoadPrefs();
            }
        }

        if(battleBtn != null)
        {
            if (CheckIfSlotsEmpty())
            {
                battleBtn.interactable = false;
            }
            else
            {
                battleBtn.interactable = true;
            }
        }
    }

    /// <summary>
    /// Called when a player selects a tank card. Updates selected tank for main menu or game context.
    /// </summary>
    /// <param name="card">The selected TankCard object.</param>
    public void SelectTank(TankCard card)
    {
        if (card.tankVarient == null) { return; }
        RespawnManager rm = FindAnyObjectByType<RespawnManager>();
        if (rm != null)
        {
            // In-game tank selection
            rm.RequestTankChange(card.tankVarient.tankName);
            HUDUI.Singleton.ShowRespawnUI();
        }
        else
        {
            // Main menu tank selection
            FindAnyObjectByType<Player>().ChangeTank(card.tankVarient); 
            MainMenu.Singleton.SetSelectedVehicleText(card.tankVarient.description);
        }
    
    }

    /// <summary>
    /// Checks all slots to determine if any contain a valid tank variant.
    /// </summary>
    /// <returns>True if all slots are empty; otherwise, false.</returns>
    public bool CheckIfSlotsEmpty()
    {
        foreach (RectTransform transform in TankList)
        {
            foreach (Transform child in transform)
            {
                if(child.GetComponent<TankCard>().tankVarient != null)
                {
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Recalculates layout width based on number of tank slots and adjusts UI for screen width.
    /// </summary>
    void RecalculateTankListWidth()
    {
        foreach(RectTransform holder in TankList)
        {
            int childCount = holder.transform.childCount;
            int width = (childCount * 215) + (9 * (childCount + 1)); // Slot width + margin

            #if UNITY_EDITOR
            Vector2 screen = GetMainGameViewSize();
            if (width < screen.x)
            {
                width = (int)screen.x;
            }
            #else
            if (width < Screen.width)
            {
                width = Screen.width;
            }
            #endif

            holder.sizeDelta = new Vector2(width, holder.parent.GetComponent<RectTransform>().rect.height);
            holder.anchoredPosition += new Vector2(10000, 0); // Push offscreen to avoid snapping artifacts

            // Fix Unity editor visual inconsistencies via delayed refocus
            #if UNITY_EDITOR
            if (EditorApplication.isPlaying) { return; }
            UnityEditor.EditorApplication.delayCall += () =>
            {
                FocusSceneView();

                UnityEditor.EditorApplication.delayCall += () =>
                {
                    FocusGameView();
                    
                    UnityEditor.EditorApplication.delayCall += () =>
                    {
                        FocusSceneView();
                        UnityEditor.EditorApplication.delayCall += () =>
                        {
                            FocusGameView();

                            UnityEditor.EditorApplication.delayCall += () =>
                            {
                                FocusSceneView();

                            };
                        };
                    };
                };
            };
            #endif

        }
        
    }

    /// <summary>
    /// Unity editor hook to dynamically rebuild slot UI when properties change.
    /// </summary>
    public void OnValidate()
    {
#if UNITY_EDITOR
        if (EditorApplication.isPlaying is false)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorApplication.isUpdating) return; // Prevents execution during asset imports
            if (BuildPipeline.isBuildingPlayer) return; // Prevents issues during builds
        }

        // Schedule object destruction to avoid Unity serialization issues
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (TankList == null) return; // Prevent null reference errors if the object was deleted
            foreach(RectTransform transform in TankList)
            {
                foreach (Transform child in transform)
                {       
                    DestroyImmediate(child.gameObject);
                }
                foreach (Transform child in transform)
                {
                    DestroyImmediate(child.gameObject);
                }
                foreach (Transform child in transform)
                {
                    DestroyImmediate(child.gameObject);
                }
                foreach (Transform child in transform)
                {
                    DestroyImmediate(child.gameObject);
                }
                if (this == null) return; // Prevent null reference errors if the object was deleted
                for (int i = 0; i < unlockedSlots; i++)
                {
                    GameObject go = Instantiate(holderPrefab, transform);
                    go.name = (i+1).ToString();
                    go.GetComponent<TankCard>().holder = true;
                    go.GetComponent<TankCard>().canChange = (SceneManager.GetActiveScene().name == "MainMenu"); // can only change in main menu
                }

                RecalculateTankListWidth();
                SavedPrefsToCards();

            }
            
        };
#endif
    }

#if UNITY_EDITOR
    /// <summary>
    /// Retrieves the size of the main game view window in the Unity editor.
    /// </summary>
    public static Vector2 GetMainGameViewSize()
    {
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        object result = GetSizeOfMainGameView.Invoke(null, null);
        return (Vector2)result;
    }

    /// <summary>
    /// Forces Unity to focus the Game view tab.
    /// </summary>
    public static void FocusGameView()
    {
        EditorApplication.ExecuteMenuItem("Window/General/Game");
    }

    /// <summary>
    /// Forces Unity to focus the Scene view tab.
    /// </summary>
    public static void FocusSceneView()
    {
        EditorApplication.ExecuteMenuItem("Window/General/Scene");
    }
#endif

}
