using Ballistics;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Central UI controller handling all heads-up display windows and elements for the player.
/// Manages visibility of HUD, sniper mode, pause, respawn, settings, and match end UI.
/// Controls health bar, score, flag status, repair UI, reload UI, and team selection buttons.
/// Implements singleton pattern for global access.
/// </summary>
public class HUDUI : MonoBehaviour
{
    public static HUDUI Singleton;
    [SerializeField] private WindowManager windowManager;
    [SerializeField] private Window hudWindow;
    [SerializeField] private Window sniperWindow;
    [SerializeField] private Window pauseWindow;
    [SerializeField] private Window respawnWindow;
    [SerializeField] private Window settingsWindow;
    [SerializeField] private Window endOfMatchWindow;
    [SerializeField] private Window tankSelectionWindow;

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

    [Space(10)]
    [SerializeField] private Image RepairHoldRadial;
    [SerializeField] private TextMeshProUGUI repairTimeText;
    [SerializeField] private GameObject repairObject;
    [SerializeField] private Slider reloadSlider;
    [SerializeField] private TextMeshProUGUI flipText;

    public Button orangeButton;
    public Button blueButton;

    /// <summary>
    /// Initializes UI singleton, shows settings window and tank selection at start.
    /// Ensures cursor is visible for menu interaction.
    /// </summary>
    void Start()
    {
        windowManager.ShowOnly(settingsWindow);
        Singleton = this;
        SetCursorShown(true);
        windowManager.ShowWindow(tankSelectionWindow);
        SetupJoinButtons();
    }

    /// <summary>
    /// Ensures Start logic is called on Awake, supporting editor or runtime calls.
    /// </summary>
    private void Awake()
    {
        Start();
    }

    /// <summary>
    /// Handles pause input to toggle pause menu or close other UI windows.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(Settings.Singleton.KeyCodeFromSetting("Control-Pause/Back")))
        {
            Debug.Log("Escape Logic");
            EscLogic();
        }
    }

    /// <summary>
    /// Shows or hides the cursor depending on game state.
    /// </summary>
    public void SetCursorShown(bool shown)
    {
        Cursor.lockState = shown ? CursorLockMode.None : CursorLockMode.Locked;
    }

    /// <summary>
    /// Centralized logic for Escape key: closes settings or pause windows,
    /// or opens pause menu if none active.
    /// Prevents escape exit from respawn or tank selection windows.
    /// </summary>
    private void EscLogic()
    {
        if (respawnWindow == null || settingsWindow == null || pauseWindow == null)
        {
            Debug.LogWarning("HUDUI is missing required variable assignments...");
            return;
        }

        if (respawnWindow.isActiveAndEnabled || tankSelectionWindow.isActiveAndEnabled)
        {
            // Cannot exit respawn or tank selection with escape
            return;
        }

        if (settingsWindow.isActiveAndEnabled)
        {
            settingsWindow.Hide();
            return;
        }

        if (pauseWindow.isActiveAndEnabled)
        {
            pauseWindow.Hide();
            SetCursorShown(false);
            return;
        }

        windowManager.ShowOnly(pauseWindow);
        SetCursorShown(true);
    }

    /// <summary>
    /// Switches UI to sniper mode window.
    /// </summary>
    public void ShowSniperMode()
    {
        if (windowManager == null || sniperWindow == null)
        {
            Debug.LogWarning("HUDUI is missing required variable assignments...");
            return;
        }
        windowManager.ShowWindow(sniperWindow);
    }

    /// <summary>
    /// Reverts UI back to normal HUD window from sniper mode.
    /// </summary>
    public void HideSniperMode()
    {
        if (windowManager == null || hudWindow == null)
        {
            Debug.LogWarning("HUDUI is missing required variable assignments...");
            return;
        }
        windowManager.ShowWindow(hudWindow);
    }

    /// <summary>
    /// Initiates network shutdown and loads main menu scene.
    /// </summary>
    public void LeaveMatch()
    {
        StartCoroutine(ShutdownThenLoadScene("MainMenu"));
    }

    /// <summary>
    /// Coroutine that cleanly disconnects network, destroys NetworkManager,
    /// and transitions to specified scene.
    /// </summary>
    private System.Collections.IEnumerator ShutdownThenLoadScene(string sceneName)
    {
        // Shut down network
        NetworkManager.Singleton.Shutdown();

        // Wait a short time to ensure disconnect messages, despawns, etc. propagate
        yield return new WaitForSeconds(0.5f);

        // Destroy the NetworkManager so it doesn't persist across scenes
        Destroy(NetworkManager.Singleton.gameObject);

        // Wait another frame to ensure Destroy is processed
        yield return null;

        // Now load the main menu scene
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Shows respawn UI window and ensures cursor is visible.
    /// Only called on clients, never on server.
    /// </summary>
    public void ShowRespawnUI()
    {
        if (NetworkManager.Singleton.IsServer) { return; }
        windowManager.ShowWindow(respawnWindow);
        SetCursorShown(true);
    }

    /// <summary>
    /// Updates health bar slider and text to reflect current health status.
    /// </summary>
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        Slider s = healthBar.GetComponent<Slider>();
        s.maxValue = maxHealth;
        s.value = currentHealth;

        int healthPercentage = (int)(currentHealth / maxHealth * 100); // int to get whole numbers (no rounding)
        healthBarText.GetComponent<TextMeshProUGUI>().text = $"{healthPercentage}% Health";
    }

    /// <summary>
    /// Updates the health bar colors based on the player's team colors.
    /// </summary>
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
    }

    

    /// <summary>
    /// Updates colors of the battle status bar to reflect local and opposing team colors.
    /// </summary>
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

    /// <summary>
    /// Updates colors of the battle status bar to reflect local and opposing team colors.
    /// </summary>
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

    /// <summary>
    /// Creates and initializes a UI element representing a capture flag on the HUD.
    /// Disabled on server as UI is client-side.
    /// </summary>
    public GameObject CreateFlagUI(string flagLetter)
    {
        if (NetworkManager.Singleton.IsServer) { return null; }
        GameObject flagUI = Instantiate(flagStatusPrefab, flagStatusHolder.transform);
        flagUI.GetComponentInChildren<TextMeshProUGUI>().text = flagLetter.ToUpper();
        flagUI.transform.GetChild(1).GetComponent<Image>().color = PlayerTeam.GetNormalColour(Team.None);
        flagUI.transform.GetChild(1).GetComponent<Image>().fillAmount = 1; // max value
        return flagUI;
    }

    /// <summary>
    /// Updates the UI flag progress and color fill to reflect capture progress and owning team.
    /// </summary>
    public void UpdateFlagUIValues(GameObject flagUI, Team colour, float progress)
    {
        if(flagUI == null) { return; }
        flagUI.transform.GetChild(1).GetComponent<Image>().fillAmount = progress;
        flagUI.transform.GetChild(1).GetComponent<Image>().color = PlayerTeam.GetNormalColour(colour);
    }

    /// <summary>
    /// Displays end of match UI window with the winning team announcement.
    /// </summary>
    public void ShowGameOver(Team winner)
    {
        windowManager.ShowWindow(endOfMatchWindow);
        endOfMatchText.text = $"{winner} team has won!";
    }

    /// <summary>
    /// (Currently empty) Intended to update UI components related to tank modules.
    /// </summary>
    public void UpdateComponentsUI(List<FunctionalTankModule> modules)
    {
        // TODO: Implement component UI update logic
    }

    /// <summary>
    /// Updates the reload progress slider UI based on normalized reload time.
    /// </summary>
    public void UpdateReloadTime(float timeZO)
    {
        if(reloadSlider != null)
        {
            reloadSlider.value = timeZO;
        }
    }

    /// <summary>
    /// Shows or hides the repair progress UI and updates its fill amount.
    /// </summary>
    public void ShowRepairUI(float value)
    {
        if(value == 0f)
        {
            repairObject.SetActive(false);
            return;
        }

        repairObject.SetActive(true);
        RepairHoldRadial.fillAmount = value;
    }

    /// <summary>
    /// Updates the textual repair timer UI.
    /// </summary>
    public void ShowRepairTimer(float value)
    {
        repairTimeText.text = $"{value}s";
    }

    /// <summary>
    /// Shows the tank selection UI window and makes the cursor visible for interaction.
    /// </summary>
    public void ShowTankSelectionUI()
    {
        windowManager.ShowWindow(tankSelectionWindow);
        SetCursorShown(true);
    }

    /// <summary>
    /// Shows or hides the flip tank prompt and updates the prompt text with the correct key binding.
    /// </summary>
    public void FlipPromptActive(bool active)
    {
        flipText.text = $"Press {Settings.Singleton.KeyCodeFromSetting("Control-FlipTank")} to flip your tank...";
        flipText.gameObject.SetActive(active);
    }

}
