using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class Initialize : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AlwaysInitalize.Initialise(); // if we reach this point then init will execute
        #if UNITY_SERVER

            // Code to execute on the server
            Debug.Log("Running on the server.");
            NetworkManager.Singleton.StartServer();
            SceneManager.LoadScene("Play");
        #else
            // Code to execute on the client
            Debug.Log("Running on the client.");
            SceneManager.LoadScene("MainMenu");
        #endif
    }
}
