using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUDUI : MonoBehaviour
{
    public static HUDUI Singleton;
    [SerializeField] private WindowManager windowManager;
    [SerializeField] private Window sniperWindow;
    [SerializeField] private Window pauseWindow;
    [SerializeField] private Window respawnWindow;
    [SerializeField] private Window settingsWindow;
    [SerializeField] private Window endOfMatchWindow;

    [Space(10)]

    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject battleStatusBar;
    [SerializeField] private GameObject healthBarText;
    [SerializeField] private GameObject localTeamBSBText;
    [SerializeField] private GameObject awayTeamBSBText;

    [Space(10)]

    [SerializeField] private GameObject flagStatusHolder;
    [SerializeField] private GameObject flagStatusPrefab;

    [Space(10)]

    [SerializeField] private TextMeshProUGUI endOfMatchText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Singleton = this;
        SetCursorShown(false);
        ShowRespawnUI();
        SetupJoinButtons();
    }

    private void Awake()
    {
        Singleton = this;
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
        StartCoroutine(ShutdownThenLoadScene("MainMenu"));
    }

    private System.Collections.IEnumerator ShutdownThenLoadScene(string sceneName)
    {
        // Shut down network
        NetworkManager.Singleton.Shutdown();

        // Optional: Wait a short time to ensure disconnect messages, despawns, etc. propagate
        yield return new WaitForSeconds(0.5f);

        // Destroy the NetworkManager so it doesn't persist across scenes
        Destroy(NetworkManager.Singleton.gameObject);

        // Wait another frame to ensure Destroy is processed
        yield return null;

        // Now load the main menu scene
        SceneManager.LoadScene(sceneName);
    }

    public void ShowRespawnUI()
    {
        if (NetworkManager.Singleton.IsServer) { return; }
        windowManager.ShowWindow(respawnWindow);
        SetCursorShown(true);
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        Slider s = healthBar.GetComponent<Slider>();
        s.maxValue = maxHealth;
        s.value = currentHealth;

        int healthPercentage = (int)(currentHealth / maxHealth * 100); // int to get whole numbers (no rounding)
        healthBarText.GetComponent<TextMeshProUGUI>().text = $"{healthPercentage}% Health";
    }

    public void UpdateTeamColour(Team team)
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

    public Button orangeButton;
    public Button blueButton;

    public void SelectTeam(Team team)
    {
        if (RespawnManager.Singleton != null && NetworkManager.Singleton.IsClient)
        {
            RespawnManager.Singleton.SelectTeamServerRpc(team);
        }
    }

    void SetupJoinButtons()
    {
        orangeButton.onClick.AddListener(() => SelectTeam(Team.Orange));
        blueButton.onClick.AddListener(() => SelectTeam(Team.Blue));
    }

    public void UpdateScore(float value)
    {
        value = Mathf.Clamp01(value); // slider value is 0->1
        battleStatusBar.GetComponent<Slider>().value = value;

        int localScore = (int)(value * 100);
        int awayScore = 100 - localScore;

        localTeamBSBText.GetComponent<TextMeshProUGUI>().text = $"{localScore}";
        awayTeamBSBText.GetComponent<TextMeshProUGUI>().text = $"{awayScore}";
    }

    public void SetTeams(Team local, Team away)
    {
        var images = battleStatusBar.GetComponentsInChildren<Image>();
        foreach (Image image in images)
        {
            if (image.gameObject.name == "Fill")
            {
                Debug.Log("Normal Color Set");
                image.color = PlayerTeam.GetNormalColour(local);
            }
            if (image.gameObject.name == "Background")
            {
                Debug.Log("Dark Color Set");
                image.color = PlayerTeam.GetNormalColour(away);
            }
        }
    }

    public GameObject CreateFlagUI(string flagLetter)
    {
        if (NetworkManager.Singleton.IsServer) { return null; }
        GameObject flagUI = Instantiate(flagStatusPrefab, flagStatusHolder.transform);
        flagUI.GetComponentInChildren<TextMeshProUGUI>().text = flagLetter.ToUpper();
        flagUI.transform.GetChild(1).GetComponent<Image>().color = PlayerTeam.GetNormalColour(Team.None);
        flagUI.transform.GetChild(1).GetComponent<Image>().fillAmount = 1; // max value
        return flagUI;
    }

    public void UpdateFlagUIValues(GameObject flagUI, Team colour, float progress)
    {
        if(flagUI == null) { return; }
        flagUI.transform.GetChild(1).GetComponent<Image>().fillAmount = progress;
        flagUI.transform.GetChild(1).GetComponent<Image>().color = PlayerTeam.GetNormalColour(colour);
    }

    public void ShowGameOver(Team winner)
    {
        windowManager.ShowWindow(endOfMatchWindow);
        endOfMatchText.text = $"{winner} team has won!";
    }
}
