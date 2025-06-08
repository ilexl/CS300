using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TankSelection : MonoBehaviour
{
    public static TankSelection Singleton;
    [SerializeField][Range(1f, 20f)] int unlockedSlots;
    [SerializeField] List<RectTransform> TankList;
    [SerializeField] GameObject holderPrefab;
    [SerializeField] Button battleBtn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Singleton = this;
        RecalculateTankListWidth();
        SavedPrefsToCards();
    }

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

    public void SelectTank(TankCard card)
    {
        if (card.tankVarient == null) { return; }
        RespawnManager rm = FindAnyObjectByType<RespawnManager>();
        if (rm != null)
        {
            // respawn manager only active in game

            // set tank to respawn in
            rm.RequestTankChange(card.tankVarient.tankName);

            // set UI to team selection
            HUDUI.Singleton.ShowRespawnUI();

        }
        else
        {
            // main menu
            
            // change display tank
            FindAnyObjectByType<Player>().ChangeTank(card.tankVarient); 
        
            // UI stats on selected tank
            MainMenu.Singleton.SetSelectedVehicleText(card.tankVarient.description);
        }
    
    }

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

        return true; // true == empty == no cards contain valid tanks
    }

    void RecalculateTankListWidth()
    {
        foreach(RectTransform holder in TankList)
        {
            int childCount = holder.transform.childCount;
            int width = (childCount * 215) + (9 * (childCount + 1)); // width of each child + border.
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
            holder.anchoredPosition += new Vector2(10000, 0);
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
    public static Vector2 GetMainGameViewSize()
    {
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        object result = GetSizeOfMainGameView.Invoke(null, null);
        return (Vector2)result;
    }

    public static void FocusGameView()
    {
        EditorApplication.ExecuteMenuItem("Window/General/Game");
    }

    public static void FocusSceneView()
    {
        EditorApplication.ExecuteMenuItem("Window/General/Scene");
    }
#endif

}
