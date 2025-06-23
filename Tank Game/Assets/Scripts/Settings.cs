using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Manages the user settings for video, audio, graphics, camera, and controls.
/// Handles UI creation, input rebinding, and application of preferences.
/// </summary>
public class Settings : MonoBehaviour
{
    public static Settings Singleton;
    [SerializeField] private GameObject settingPrefabUI;
    [SerializeField] private Transform videoHolder, graphicsHolder, audioHolder, controlsHolder, cameraHolder, otherHolder;
    [SerializeField] private GameObject controlRebindablePrefab;
    [SerializeField] private Window ChangeControlWindow;
    List<Setting> settings;

    /// <summary>
    /// Called at start; initializes the singleton and loads settings.
    /// </summary>
    void Start()
    {
        Singleton = this;
        LoadSettings();
    }

    /// <summary>
    /// Unity Awake method override; defers to Start for initialization.
    /// </summary>
    private void Awake()
    {
        Start();
    }

    /// <summary>
    /// Displays the change control UI and handles input rebinding for a setting.
    /// </summary>
    public void ShowChangeControl(Setting setting, Action callback)
    {
        ChangeControlWindow.Show();

        // Start listening for input from user
        StartCoroutine(WaitForControlInput(input =>
        {
            setting.UpdateCurrentValue(input);
            callback?.Invoke();
            ChangeControlWindow.Hide();
        }));
    }

    /// <summary>
    /// Coroutine that waits until a key is pressed, then passes it to a callback.
    /// </summary>
    private IEnumerator WaitForControlInput(Action<string> onInputReceived)
    {
        bool inputReceived = false;
        string detectedInput = null;

        while (!inputReceived)
        {
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    detectedInput = key.ToString();
                    inputReceived = true;
                    break;
                }
            }

            // Could be extended for controller, mouse input, etc.
            Debug.Log("Waiting for input");
            yield return null; // wait until next frame
        }

        onInputReceived?.Invoke(detectedInput);
    }

    /// <summary>
    /// Builds a predefined list of all game settings with default values.
    /// </summary>
    private List<Setting> GetSettingsList()
    {
        List<Setting> list = new List<Setting>();

        // Video
        list.Add(new Setting("Video-Resolution", "HIGHEST")); // Resolution
        list.Add(new Setting("Video-Fullscreen", "Borderless")); // Fullscreen / Borderless / Windowed
        list.Add(new Setting("Video-Aspect Ratio", "16:9")); // Aspect Ratio -> TESTING REQUIRED (otherwise cannot change from 16:9)
        list.Add(new Setting("Video-VSync", "on")); // VSYNC
        list.Add(new Setting("Video-RefreshRate", "HIGHEST")); // Refresh Rate

        // Graphics
        // TODO: determine graphics default values
        list.Add(new Setting("Graphics-TextureQuality", "TBC")); // Texture quality
        list.Add(new Setting("Graphics-ShadowQuality", "TBC")); // Shadow quality
        list.Add(new Setting("Graphics-ReflectionQuality", "TBC")); // Reflection quality
        list.Add(new Setting("Graphics-ParticleQuality", "TBC")); // Particle effect quality(visual effects, explosions, atmospheric effects, volumetric clouds, fog, water)
        list.Add(new Setting("Graphics-AntiAliasing", "TBC")); // Anti-Aliasing
        list.Add(new Setting("Graphics-AmbiantOcclusion", "TBC")); // Ambiant Occlusion
        list.Add(new Setting("Graphics-DepthOfField", "TBC")); // Depth of Field

        // Audio
        list.Add(new Setting("Audio-Master", "100")); // Master Audio
        list.Add(new Setting("Audio-Effects", "100")); // Sound Effects
        list.Add(new Setting("Audio-Engines", "100")); // Engine Sounds (Min 40%)
        list.Add(new Setting("Audio-MusicMenu", "100")); // Main Menu Music
        list.Add(new Setting("Audio-MusicGame", "100")); // In Game Music

        // Controls (Rebindable)
        list.Add(new Setting("Control-Forward", "W")); // W Forward
        list.Add(new Setting("Control-Left", "A")); // A Left
        list.Add(new Setting("Control-Back", "S")); // S Back
        list.Add(new Setting("Control-Right", "D")); // D Right
        list.Add(new Setting("Control-Freelook", "C")); // C Freelook
        list.Add(new Setting("Control-Repair", "R")); // R Repair
        list.Add(new Setting("Control-FlipTank", "F")); // F Flip Tank
        list.Add(new Setting("Control-ShootPrimary", "Mouse0")); // M1 Shoot (Cannons)
        list.Add(new Setting("Control-ShootSecondary", "Space")); // Spacebar Shoot (Machine Gun)
        list.Add(new Setting("Control-CameraZoom", "Mouse1")); // M2 Aim (Zoom)
        list.Add(new Setting("Control-SniperMode", "LeftShift")); // Shift Aim (Sniper)
        list.Add(new Setting("Control-Pause/Back", "Escape")); // Esc Back
        // ...

        // Camera
        list.Add(new Setting("Camera-FOV", "60")); // Field of view
        list.Add(new Setting("Camera-LookSensitivity", "1.0")); // Look Around (Sensitivity)
        list.Add(new Setting("Camera-AimSensitivity", "1.0")); // Aim (Sensitivity)
        list.Add(new Setting("Camera-AxisInverted", "false")); // Inverted aim (bool)

        // Other
        // .. anything else that pops up here

        return list;
    }

    /// <summary>
    /// Retrieves valid options for a given setting, based on its name.
    /// </summary>
    private List<string> GetSettingOptions(Setting setting)
    {
        List<string> options = new List<string>();

        // Video
        #region Video
        if (setting.GetName() == "Video-Resolution")
        {
            HashSet<string> resolutionSet = new HashSet<string>();
            List<string> resolutions = new List<string>();

            foreach (Resolution res in Screen.resolutions)
            {
                string resKey = $"{res.width}x{res.height}";
                if (!resolutionSet.Contains(resKey))
                {
                    resolutionSet.Add(resKey);
                    resolutions.Add(resKey);
                }
            }

            Debug.Log("Available Resolutions:");
            foreach (string r in resolutions)
            {
                options.Add(r);
                Debug.Log(r);
            }
        }
        if (setting.GetName() == "Video-Fullscreen")
        {
            options.Add("Fullscreen");
            options.Add("Borderless");
            options.Add("Windowed");
        }
        if (setting.GetName() == "Video-Aspect")
        {
            // unsure yet
            // TODO: determine if Video-Aspect can be deleted?
        }
        if (setting.GetName() == "Video-VSync")
        {
            options.Add("Off");
            options.Add("Half");
            options.Add("On");
        }
        if (setting.GetName() == "Video-RefreshRate")
        {
            HashSet<string> refreshRateSet = new HashSet<string>();
            List<string> refreshRates = new List<string>();

            foreach (Resolution res in Screen.resolutions)
            {
                string rateKey = $"{res.refreshRateRatio}Hz";

                if (!refreshRateSet.Contains(rateKey))
                {
                    refreshRateSet.Add(rateKey);
                    refreshRates.Add(rateKey);
                }
            }

            Debug.Log("Available Refresh Rates:");
            foreach (string hz in refreshRates)
            {
                options.Add(hz);
                Debug.Log(hz);
            }
        }
        #endregion

        // Graphics
        #region Graphics
        if (setting.GetName() == "Graphics-TextureQuality")
        {

        }
        if (setting.GetName() == "Graphics-ShadowQuality")
        {

        }
        if (setting.GetName() == "Graphics-ReflectionQuality")
        {

        }
        if (setting.GetName() == "Graphics-ParticleQuality")
        {

        }
        if (setting.GetName() == "Graphics-AntiAliasing")
        {

        }
        if (setting.GetName() == "Graphics-AmbiantOcclusion")
        {

        }
        if (setting.GetName() == "Graphics-DepthOfField")
        {

        }
        #endregion

        // Audio
        #region Audio
        if (setting.GetName() == "Audio-Master" || setting.GetName() == "Audio-Effects" || setting.GetName() == "Audio-MusicMenu" || setting.GetName() == "Audio-MusicGame")
        {
            options.Add("0");
            options.Add("10");
            options.Add("20");
            options.Add("30");
            options.Add("40");
            options.Add("50");
            options.Add("60");
            options.Add("70");
            options.Add("80");
            options.Add("90");
            options.Add("100");
        }
        if (setting.GetName() == "Audio-Engines")
        {
            options.Add("40");
            options.Add("50");
            options.Add("60");
            options.Add("70");
            options.Add("80");
            options.Add("90");
            options.Add("100");
        }
        #endregion

        // Camera
        #region Camera
        if (setting.GetName() == "Camera-FOV")
        {
            options.Add("40");
            options.Add("45");
            options.Add("50");
            options.Add("55");
            options.Add("60");
            options.Add("65");
            options.Add("70");
            options.Add("75");
            options.Add("80");
            options.Add("85");
            options.Add("90");
            options.Add("95");
            options.Add("100");
        }
        if (setting.GetName() == "Camera-LookSensitivity")
        {
            options.Add("1");
        }
        if (setting.GetName() == "Camera-AimSensitivity")
        {
            options.Add("1");
        }
        if (setting.GetName() == "Camera-AxisInverted")
        {
            options.Add("false");
            options.Add("true");
        }
        #endregion

        // Control
        #region Control
        if (setting.GetName().Contains("Control-"))
        {

        }
        #endregion

        if (options.Count == 0) { Debug.LogWarning($"Setting {setting.GetName()} has no options, this needs to be set in GetSettingsOptions manually..."); }
        return options;
    }

    /// <summary>
    /// Destroys all existing UI setting elements from each category container.
    /// </summary>
    private void DeleteUI()
    {
#if UNITY_EDITOR
        // Schedule object destruction to avoid Unity serialization issues
        if (EditorApplication.isPlaying)
        {
            foreach (Transform child in videoHolder) { Destroy(child.gameObject, 0.01f); }
            foreach (Transform child in graphicsHolder) { Destroy(child.gameObject, 0.01f); }
            foreach (Transform child in audioHolder) { Destroy(child.gameObject, 0.01f); }
            foreach (Transform child in controlsHolder) { Destroy(child.gameObject, 0.01f); }
            foreach (Transform child in cameraHolder) { Destroy(child.gameObject, 0.01f); }
            foreach (Transform child in otherHolder) { Destroy(child.gameObject, 0.01f); }
        }
        else
        {
            #region EDITOR ONLY
            
            if (EditorApplication.isUpdating) return; // Prevents execution during asset imports
            if (BuildPipeline.isBuildingPlayer) return; // Prevents issues during builds

            foreach (Transform child in videoHolder)    { if (child.gameObject == null) { continue; } UnityEditor.EditorApplication.delayCall += () => { DestroyImmediate(child.gameObject); }; }
            foreach (Transform child in graphicsHolder) { if (child.gameObject == null) { continue; } UnityEditor.EditorApplication.delayCall += () => { DestroyImmediate(child.gameObject); }; }
            foreach (Transform child in audioHolder)    { if (child.gameObject == null) { continue; } UnityEditor.EditorApplication.delayCall += () => { DestroyImmediate(child.gameObject); }; }
            foreach (Transform child in controlsHolder) { if (child.gameObject == null) { continue; } UnityEditor.EditorApplication.delayCall += () => { DestroyImmediate(child.gameObject); }; }
            foreach (Transform child in cameraHolder)   { if (child.gameObject == null) { continue; } UnityEditor.EditorApplication.delayCall += () => { DestroyImmediate(child.gameObject); }; }
            foreach (Transform child in otherHolder)    { if (child.gameObject == null) { continue; } UnityEditor.EditorApplication.delayCall += () => { DestroyImmediate(child.gameObject); }; }


            #endregion
        }
#else
        foreach (Transform child in videoHolder) { Destroy(child.gameObject, 0.01f); }
        foreach (Transform child in graphicsHolder) { Destroy(child.gameObject, 0.01f); }
        foreach (Transform child in audioHolder) { Destroy(child.gameObject, 0.01f); }
        foreach (Transform child in controlsHolder) { Destroy(child.gameObject, 0.01f); }
        foreach (Transform child in cameraHolder) { Destroy(child.gameObject, 0.01f); }
        foreach (Transform child in otherHolder) { Destroy(child.gameObject, 0.01f); }
#endif
    }

    /// <summary>
    /// Creates a fresh UI for all settings and binds them to their respective controls.
    /// </summary>
    private void NewUI()
    {
        // delete old UI
        DeleteUI();

        if (settingPrefabUI == null)
        {
            Debug.LogError("Settings Prefab is its UI is missing, cannot continue!");
            return;
        }


        // create new UI
        string currentType = "";
        foreach(Setting setting in settings)
        {
            Transform parentType = null;
            if(setting.GetName().Contains('-') is false)
            {
                Debug.LogWarning($"Setting {setting.GetName()} is not a valid name for a setting...");
                continue;
            }
            string type = (setting.GetName().Split('-'))[0];

            // alternating list colours need to be reset here for each category created
            if (currentType != type && type != "Control")
            {
                currentType = type;
                SettingUI.ResetColourCount();
            }
            else if (currentType != type && type == "Control")
            {
                currentType = type;
                EditableControl.ResetColourCount();
            }

            if (type == "Control") { parentType = controlsHolder; }
            if (parentType != null)
            {
                // control needs it own script as it is special...
                GameObject ui = Instantiate(controlRebindablePrefab, parentType);
                ui.name = setting.GetName();
                ui.GetComponent<EditableControl>().Setup(setting);
                continue;
            }

            if (type == "Video") { parentType = videoHolder; }
            if (type == "Graphics") { parentType = graphicsHolder; }
            if (type == "Audio") { parentType = audioHolder; }
            if (type == "Camera") { parentType = cameraHolder; }
            if (type == "Other") { parentType = otherHolder; }

            if(parentType != null)
            {
                GameObject ui = Instantiate(settingPrefabUI, parentType);
                ui.name = setting.GetName();
                ui.GetComponent<SettingUI>().Setup(setting, GetSettingOptions(setting));
                continue;
            }

            

        }

    }

    /// <summary>
    /// Loads all saved settings from PlayerPrefs and generates the settings UI.
    /// </summary>
    public void LoadSettings()
    {
        settings = GetSettingsList();
        foreach(Setting setting in settings)
        {
            setting.LoadSetting();
        }

        NewUI();
    }

    /// <summary>
    /// Saves all current settings to PlayerPrefs.
    /// </summary>
    public void SaveSettings()
    {
        foreach (Setting setting in settings)
        {
            setting.SaveSetting();
        }
    }

    /// <summary>
    /// Clears all saved settings and reverts to default.
    /// </summary>
    public void ResetSettings()
    {
        foreach(Setting setting in settings)
        {
            setting.ResetSetting();
        }
    }

    /// <summary>
    /// Applies current settings values to the game systems.
    /// </summary>
    public void ApplySettings()
    {
        int targetWidth = Screen.currentResolution.width;
        int targetHeight = Screen.currentResolution.height;
        double targetRefreshRate = Screen.currentResolution.refreshRateRatio.value;
        FullScreenMode targetFullScreenMode = Screen.fullScreenMode;


        foreach (Setting setting in settings)
        {
            string name = setting.GetName();
            string value = setting.GetCurrentValue();

            switch (name)
            {
                // Video
                case "Video-Resolution":
                    string[] res = value.Split('x');
                    if (res.Length == 2 && int.TryParse(res[0], out int width) && int.TryParse(res[1], out int height))
                    {
                        targetWidth = width;
                        targetHeight = height;
                    }
                    break;

                case "Video-Fullscreen":
                    switch (value)
                    {
                        case "Fullscreen":
                            targetFullScreenMode = FullScreenMode.ExclusiveFullScreen;
                            break;
                        case "Borderless":
                            targetFullScreenMode = FullScreenMode.FullScreenWindow;
                            break;
                        case "Windowed":
                            targetFullScreenMode = FullScreenMode.Windowed;
                            break;
                    }
                    break;

                case "Video-VSync":
                    switch (value)
                    {
                        case "Off": QualitySettings.vSyncCount = 0; break;
                        case "Half": QualitySettings.vSyncCount = 2; break;
                        case "On": QualitySettings.vSyncCount = 1; break;
                    }
                    break;

                case "Video-RefreshRate":
                    if (double.TryParse(value, out double refreshRate))
                    {
                        targetRefreshRate = refreshRate;
                    }
                    break;

                // Graphics
                case "Graphics-TextureQuality":
                    // Example: map string to int
                    // QualitySettings.masterTextureLimit = value == "Low" ? 2 : value == "Medium" ? 1 : 0;
                    break;

                case "Graphics-ShadowQuality":
                    // QualitySettings.shadows = ShadowQuality.All;
                    break;

                case "Graphics-ReflectionQuality":
                    break;

                case "Graphics-ParticleQuality":
                    break;

                case "Graphics-AntiAliasing":
                    break;

                case "Graphics-AmbiantOcclusion":
                    break;

                case "Graphics-DepthOfField":
                    break;

                // Audio
                case "Audio-Master":
                    // AudioListener.volume = int.Parse(value) / 100f;
                    break;
                case "Audio-Effects":
                    // AudioManager.Instance.SetVolume("Effects", int.Parse(value) / 100f);
                    break;
                case "Audio-Engines":
                    // AudioManager.Instance.SetVolume("Engines", int.Parse(value) / 100f);
                    break;
                case "Audio-MusicMenu":
                    // AudioManager.Instance.SetVolume("MusicMenu", int.Parse(value) / 100f);
                    break;
                case "Audio-MusicGame":
                    // AudioManager.Instance.SetVolume("MusicGame", int.Parse(value) / 100f);
                    break;

                // Camera
                case "Camera-FOV":
                    Camera.main.fieldOfView = float.Parse(value);
                    break;
                case "Camera-LookSensitivity":
                    // PlayerController.LookSensitivity = float.Parse(value);
                    break;
                case "Camera-AimSensitivity":
                    // PlayerController.AimSensitivity = float.Parse(value);
                    break;
                case "Camera-AxisInverted":
                    // PlayerController.InvertY = bool.Parse(value);
                    break;

                // Controls
                default:
                    {
                        break;
                    }
            }
        }


        ApplyResolution(targetWidth, targetHeight, targetFullScreenMode, targetRefreshRate);
    }

    /// <summary>
    /// Applies a resolution and refresh rate using Unity's resolution API.
    /// </summary>
    public void ApplyResolution(int width, int height, FullScreenMode mode, double targetHz)
    {
        // Try to find the closest supported refresh rate
        RefreshRate closestRate = Screen.resolutions
            .Where(r => r.width == width && r.height == height)
            .OrderBy(r => Math.Abs(r.refreshRateRatio.value - targetHz))
            .FirstOrDefault()
            .refreshRateRatio;

        // Apply resolution using RefreshRate
        Screen.SetResolution(width, height, mode, closestRate);
    }

    /// <summary>
    /// Returns the KeyCode for a named control setting.
    /// </summary>
    public KeyCode KeyCodeFromSetting(string settingName)
    {
        foreach(Setting setting in settings)
        {
            if(setting.GetName() == settingName)
            {
                try
                {
                    KeyCode keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), setting.GetCurrentValue(), true);
                    return keyCode;
                }
                catch
                {
                    Debug.LogWarning($"No KeyCode found by name of {setting.GetCurrentValue()}");
                    return KeyCode.None;
                }
            }
        }
        Debug.LogWarning($"No setting found by name of {settingName}");
        return KeyCode.None;
    }

    /// <summary>
    /// Placeholder for testing experimental code.
    /// </summary>
    public void TestCodeSnippet()
    {
        
    }

    /// <summary>
    /// Represents a single configurable setting, its state and persistence.
    /// </summary>
    public class Setting
    {
        string name, defaultValue, currentValue;
        public Setting(string name, string defaultValue)
        {
            this.name = name;
            this.defaultValue = defaultValue;
            LoadSetting();
        }
        public void SaveSetting() { SaveSetting(name, currentValue); }
        public void LoadSetting() { currentValue = LoadSetting(name, defaultValue); }
        public void UpdateCurrentValue(string value) { currentValue = value; }
        public string GetCurrentValue() { return currentValue; }
        public string GetName() { return name; }
        private void SaveSetting(string key, string value) { PlayerPrefs.SetString($"SETTING-{key}", value); }
        private string LoadSetting(string key, string defaultValue) { return PlayerPrefs.GetString($"SETTING-{key}", defaultValue); }
        public void ResetSetting() { PlayerPrefs.DeleteKey($"SETTING-{name}"); }
    }
}



#if UNITY_EDITOR
/// <summary>
/// Adds custom buttons to the Unity Inspector for testing Settings at edit-time.
/// </summary>
[CustomEditor(typeof(Settings))]
public class EDITOR_Settings : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Settings settings = (Settings)target;

        if(GUILayout.Button("Load Settings")) { settings.LoadSettings(); }
        if(GUILayout.Button("Save Settings")) { settings.SaveSettings(); }
        if(GUILayout.Button("Reset Settings")) { settings.ResetSettings(); }
        if(GUILayout.Button("Apply Settings")) { settings.ApplySettings(); }
        if(GUILayout.Button("Test Code Snippet")) { settings.TestCodeSnippet(); }
    }
}


#endif