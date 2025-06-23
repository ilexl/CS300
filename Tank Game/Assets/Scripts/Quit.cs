using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Handles safe quitting of the game. Exits playmode in the editor or quits the application in a build.
/// </summary>
public class Quit : MonoBehaviour
{
    /// <summary>
    /// Safely quits the application. In the Unity Editor, exits playmode. In a build, closes the application.
    /// </summary>
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
