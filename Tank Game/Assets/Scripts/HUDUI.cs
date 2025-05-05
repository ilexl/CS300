using UnityEngine;

public class HUDUI : MonoBehaviour
{
    public static HUDUI current;
    [SerializeField] private WindowManager windowManager;
    [SerializeField] private Window sniperWindow;
    [SerializeField] private Window pauseWindow;
    [SerializeField] private Window respawnWindow;
    [SerializeField] private Window settingsWindow;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        current = this;
        SetCursorShown(false);
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
}
