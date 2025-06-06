using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TankSelection : MonoBehaviour
{
    [SerializeField][Range(1f, 20f)] int unlockedSlots;
    [SerializeField] List<RectTransform> TankList;
    [SerializeField] GameObject holderPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RecalculateTankListWidth();
    }

    public void UpdateAtIndex(int index)
    {
        foreach (Transform t in TankList)
        {
            int children = t.childCount;
            if(children != unlockedSlots) { Debug.LogError("slots != unlocked slots"); }
        }
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
                    go.name = i.ToString();
                    go.GetComponent<TankCard>().holder = true;
                    go.GetComponent<TankCard>().canChange = (SceneManager.GetActiveScene().name == "MainMenu"); // can only change in main menu
                }

                RecalculateTankListWidth();

                
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
