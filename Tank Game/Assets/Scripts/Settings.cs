using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Settings : MonoBehaviour
{
    [SerializeField] private GameObject settingPrefabUI;
    [SerializeField] private Transform videoHolder, graphicsHolder, audioHolder, controlsHolder, cameraHolder, otherHolder;
    List<Setting> settings;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadSettings();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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
        list.Add(new Setting("Graphics-Ambiant Occlusion", "TBC")); // Ambiant Occlusion
        list.Add(new Setting("Graphics-Depth of Field", "TBC")); // Depth of Field

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
        list.Add(new Setting("Control-ShootPrimary", "Mouse 0")); // M1 Shoot (Cannons)
        list.Add(new Setting("Control-ShootSecondary", "Spacebar")); // Spacebar Shoot (Machine Gun)
        list.Add(new Setting("Control-CameraZoom", "Mouse 1")); // M2 Aim (Zoom)
        list.Add(new Setting("Control-SniperMode", "Shift")); // Shift Aim (Sniper)
        list.Add(new Setting("Control-name", "Esc")); // Esc Back
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

    private void NewUI()
    {
        // delete old UI
        foreach(Transform child in videoHolder) { Destroy(child, 0.01f); }
        foreach(Transform child in graphicsHolder) { Destroy(child, 0.01f); }
        foreach(Transform child in audioHolder) { Destroy(child, 0.01f); }
        foreach(Transform child in controlsHolder) { Destroy(child, 0.01f); }
        foreach(Transform child in cameraHolder) { Destroy(child, 0.01f); }
        foreach(Transform child in otherHolder) { Destroy(child, 0.01f); }

        if(settingPrefabUI == null)
        {
            Debug.LogError("Settings Prefab is its UI is missing, cannot continue!");
            return;
        }

        // create new UI
        foreach(Setting setting in settings)
        {
            Transform parentType = null;
            if(setting.GetName().Contains('-') is false)
            {
                Debug.LogWarning($"Setting {setting.GetName()} is not a valid name for a setting...");
                continue;
            }
            string type = (setting.GetName().Split('-'))[0];
            if (type == "Video") { parentType = videoHolder; }
            if (type == "Graphics") { parentType = graphicsHolder; }
            if (type == "Audio") { parentType = audioHolder; }
            if (type == "Controls") { parentType = controlsHolder; }
            if (type == "Camera") { parentType = cameraHolder; }
            if (type == "Other") { parentType = otherHolder; }
            if (type == "Other") { parentType = otherHolder; }
            GameObject ui = Instantiate(settingPrefabUI, parentType);
            ui.name = setting.GetName();
        }

    }

    public void LoadSettings()
    {
        settings = GetSettingsList();
        foreach(Setting setting in settings)
        {
            setting.LoadSetting();
        }

        NewUI();
    }
    public void SaveSettings()
    {
        foreach (Setting setting in settings)
        {
            setting.SaveSetting();
        }
    }
    public void ResetSettings()
    {
        foreach(Setting setting in settings)
        {
            setting.ResetSetting();
        }
    }
    public void ApplySettings()
    {
        // tbc for each individual setting...
    }

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
    }
}


#endif