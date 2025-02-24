using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientNetworkUI : MonoBehaviour
{
    public void JoinServer()
    {
        SceneManager.LoadScene("Play");
    }

    public void ExitServer()
    {
        SceneManager.LoadScene("MainMenu");
    }

}
