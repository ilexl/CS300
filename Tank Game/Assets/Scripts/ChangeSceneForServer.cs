using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Redirects the server to the "Dev" scene if no main camera is found.
/// Used to ensure servers are always running in the correct headless scene.
/// </summary>
public class ChangeSceneForServer : MonoBehaviour
{
    /// <summary>
    /// At start, checks if the main camera exists.
    /// If not (likely headless/server mode), loads the development scene.
    /// </summary>
    void Start()
    {
        if(Camera.main == null) { SceneManager.LoadScene("Dev"); }   
    }
}
