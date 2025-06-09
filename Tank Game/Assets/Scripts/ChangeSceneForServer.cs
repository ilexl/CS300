using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneForServer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(Camera.main == null) { SceneManager.LoadScene("Dev"); }   
    }
}
