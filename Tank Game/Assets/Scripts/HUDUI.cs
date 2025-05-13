using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUDUI : MonoBehaviour
{
    public static HUDUI current;
    [SerializeField] private WindowManager windowManager;
    [SerializeField] private Window sniperWindow;
    [SerializeField] private Window pauseWindow;
    [SerializeField] private Window respawnWindow;
    [SerializeField] private Window settingsWindow;

    [Space(10)]

    [SerializeField] private GameObject healthBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        current = this;
        SetCursorShown(false);
        ShowRespawnUI();
    }

    private void Awake()
    {
        current = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EscLogic();
        }
    }

    public void SetCursorShown(bool shown)
    {
        if (shown)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void EscLogic()
    {
        if (respawnWindow == null || settingsWindow == null || pauseWindow == null)
        {
            Debug.LogWarning("HUDUI is missing required variable assignments...");
            return;
        }

        // check respawn
        if (respawnWindow.isActiveAndEnabled)
        {
            // do nothing as respawn cannot be exited out of with esc
            return;
        }

        // check settings
        if (settingsWindow.isActiveAndEnabled)
        {
            // close settings and hide that window
            // same as the back button
            settingsWindow.Hide();
            return;
        }

        // check paused
        if (pauseWindow.isActiveAndEnabled)
        {
            pauseWindow.Hide();
            SetCursorShown(false);
            return;
        }

        // if no windows are active then show pause menu
        windowManager.ShowOnly(pauseWindow);
        SetCursorShown(true);
    }

    public void ShowSniperMode()
    {
        if (windowManager == null || sniperWindow == null)
        {
            Debug.LogWarning("HUDUI is missing required variable assignments...");
            return;
        }
        windowManager.ShowOnly(sniperWindow);
    }

    public void HideSniperMode()
    {
        if (windowManager == null || sniperWindow == null)
        {
            Debug.LogWarning("HUDUI is missing required variable assignments...");
            return;
        }
        windowManager.HideOnly(sniperWindow);
    }

    public void LeaveMatch()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ShowRespawnUI()
    {
        windowManager.ShowWindow(respawnWindow);
        SetCursorShown(true);
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        Slider s = healthBar.GetComponent<Slider>();
        s.value = currentHealth;
        s.maxValue = maxHealth;
    }

    public void UpdateTeamColour(PlayerTeam.Side team)
    {
        // update health bar
        var images = healthBar.GetComponentsInChildren<Image>();
        foreach (Image image in images)
        {
            if (image.gameObject.name == "Fill")
            {
                Debug.Log("Normal Color Set");
                image.color = PlayerTeam.GetNormalColour(team);
            }
            if (image.gameObject.name == "Background")
            {
                Debug.Log("Dark Color Set");
                image.color = PlayerTeam.GetDarkerColour(team);
            }
        }

        // update team tracker thingy
    }
}
