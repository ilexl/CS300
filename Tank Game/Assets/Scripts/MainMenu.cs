using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    bool searching = false;
    public void StartMatchMaking()
    {
        searching = true;
        StartCoroutine(TryConnectToServer("dev.legner.foo", 7777));
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        searching = false;
    }

    // Update is called once per frame
    void Update()
    {
        
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
