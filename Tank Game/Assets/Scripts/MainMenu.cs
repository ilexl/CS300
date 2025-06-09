using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI selectedVehicleText;
    public static MainMenu Singleton;
    bool searching = false;
    public void StartMatchMaking()
    {
        searching = true;
        StartCoroutine(TryConnectToServer("dev.legner.foo", 7777));
    }

    public void SetSelectedVehicleText(string text)
    {
        selectedVehicleText.text = text;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        searching = false;
        Singleton = this;
    }

    private System.Collections.IEnumerator TryConnectToServer(string ip, int port)
    {
        while (searching)
        {
            var task = URLToIP.IsPortOpenAsync(ip, port);
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
