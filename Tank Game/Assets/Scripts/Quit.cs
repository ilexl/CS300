using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class Quit : MonoBehaviour
{
    public void SafeQuit()
    {
        Debug.Log(">>> User has pressed the Quit button <<<");
        Debug.Log("Application exiting safely!");
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
        return;
#else
        Application.Quit();
        return;
#endif
    }
}
