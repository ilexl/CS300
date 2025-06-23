using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the main menu UI and matchmaking process.
/// Handles vehicle selection display, shows a searching-for-server indicator,
/// and attempts to connect to a game server repeatedly until successful,
/// then loads the game scene.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI selectedVehicleText;

    // Singleton instance for easy access from other scripts
    public static MainMenu Singleton;

    // Flag to indicate if matchmaking is currently active
    bool searching = false;

    [SerializeField] GameObject SearchingForServerWindow;

    /// <summary>
    /// Starts the matchmaking process by showing the "searching" UI
    /// and attempting to connect to the specified server.
    /// </summary>
    public void StartMatchMaking()
    {
        searching = true;
        SearchingForServerWindow.SetActive(true);
        StartCoroutine(TryConnectToServer("dev.legner.foo", ServerHeartbeat.port));
    }

    /// <summary>
    /// Updates the UI text to show the selected vehicle's name.
    /// </summary>
    public void SetSelectedVehicleText(string text)
    {
        selectedVehicleText.text = text;
    }

    /// <summary>
    /// Initialization: hides the searching UI and sets the singleton instance.
    /// </summary>
    void Start()
    {
        SearchingForServerWindow.SetActive(false);
        searching = false;
        Singleton = this;
    }

    /// <summary>
    /// Coroutine that attempts to connect to the server repeatedly while searching is active.
    /// Waits for the server heartbeat check to complete, and if successful, loads the game scene.
    /// Retries every 2 seconds on failure.
    /// </summary>
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

                // Load the main game scene after successful connection
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
