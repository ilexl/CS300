using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI selectedVehicleText;
    public static MainMenu Singleton;
    bool searching = false;
    [SerializeField] GameObject SearchingForServerWindow;
    public void StartMatchMaking()
    {
        searching = true;
        SearchingForServerWindow.SetActive(true);
        StartCoroutine(TryConnectToServer("dev.legner.foo", ServerHeartbeat.port));
    }

    public void SetSelectedVehicleText(string text)
    {
        selectedVehicleText.text = text;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SearchingForServerWindow.SetActive(false);
        searching = false;
        Singleton = this;
    }

    private System.Collections.IEnumerator TryConnectToServer(string ip, int port)
    {
        while (searching)
        {
            var task = URLToIP.IsUnityServerAlive(ip, port);
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.Result)
            {
                Debug.Log($"Port {port} on {ip} is open. Proceeding to connect...");
                searching = false;

                SceneManager.LoadScene("Dev");

                break;
            }
            else
            {
                Debug.Log($"Port {port} on {ip} is closed. Retrying in 2 seconds...");
                yield return new WaitForSeconds(2f);
            }
        }
    }

    
}
