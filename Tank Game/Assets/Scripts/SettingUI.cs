using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI component representing a single setting option with selectable values.
/// Handles UI initialization, formatting, and cycling through options.
/// </summary>
public class SettingUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI option;
    List<string> options;
    int currentSelection = 0;
    Settings.Setting setting;

    public void Setup(Settings.Setting setting, List<string> options)
    {
        if (title == null || option == null) { Debug.LogError("SettingUI Prefab is mising title and option references and will create errors..."); return; }
        if (setting == null) { Debug.LogError("SettingUI recieved a null setting and cannot function correctly!"); return; }

        this.setting = setting;
        this.options = options;

        GetCurrentSelection();
        title.text = CleanAndSplitCamelCase(setting.GetName());
        UpdateText();

        // Assign alternating background color
        GetComponent<RawImage>().color = GetColour();
    }

    /// <summary>
    /// Determines the current selection index based on the saved setting value.
    /// </summary>
    void GetCurrentSelection()
    {
        if(setting.GetCurrentValue() == "HIGHEST")
        {
            // Use last option assuming it is the highest value
            // TODO: test if highest values ARE the last in their options lists...
            currentSelection = options.Count - 1;
            setting.UpdateCurrentValue(options[currentSelection]);
            setting.SaveSetting();
            return;
        }

        currentSelection = options.IndexOf(setting.GetCurrentValue());
        if (currentSelection == -1)
        {
            Debug.LogWarning($"Default option for setting {setting.GetName()} is NOT AN OPTION AT ALL... Will show value 0 instead for now...");
            currentSelection = 0;
            return;
        }
    }

    /// <summary>
    /// Moves the selection to the previous option in the list, wrapping around if needed.
    /// </summary>
    public void PreviousOption()
    {
        // decrement around list of options

        currentSelection--;
        if (currentSelection < 0)
        {
            currentSelection = options.Count - 1;
        }
        UpdateText();
    }

    /// <summary>
    /// Moves the selection to the next option in the list, wrapping around if needed.
    /// </summary>
    public void NextOption()
    {
        // increment around list of options

        currentSelection++;
        if (currentSelection >= options.Count)
        {
            currentSelection = 0;
        }
        UpdateText();
    }

    /// <summary>
    /// Updates the UI text to reflect the currently selected option.
    /// </summary>
    void UpdateText()
    {
        if(options.Count == 0)
        {
            Debug.LogWarning($"Index is out of range because there is no options for setting {setting.GetName()} to display...");
            option.text = "NULL";
            return;
        }
        option.text = options[currentSelection];
    }

    /// <summary>
    /// Formats the setting name by removing the prefix and splitting camel case words.
    /// </summary>
    string CleanAndSplitCamelCase(string input)
    {
        // Remove prefix before dash
        int dashIndex = input.IndexOf('-');
        if (dashIndex != -1)
        {
            input = input.Substring(dashIndex + 1);
        }

        // Skip split if input is all caps or has only one capital letter
        int uppercaseCount = 0;
        int maxConsecutiveUppercase = 0;
        int currentConsecutive = 0;

        foreach (char c in input)
        {
            if (char.IsUpper(c))
            {
                uppercaseCount++;
                currentConsecutive++;
                if (currentConsecutive > maxConsecutiveUppercase)
                    maxConsecutiveUppercase = currentConsecutive;
            }
            else
            {
                currentConsecutive = 0;
            }
        }

        if (uppercaseCount <= 1 || maxConsecutiveUppercase > 1)
        {
            return input;
        }

        // Insert spaces before capital letters
        return Regex.Replace(input, "(?<!^)([A-Z])", " $1");
    }

    const string COLORODD = "3D3D3D";
    const string COLOREVEN = "4D4D4D";
    static int colourCount = 0;

    /// <summary>
    /// Resets the alternating colour count used for background coloring.
    /// </summary>
    public static void ResetColourCount()
    {
        colourCount = 0;
    }

    /// <summary>
    /// Returns the next alternating color for background display.
    /// </summary>
    public static Color GetColour()
    {
        Color color;
        if(colourCount % 2 == 0) { ColorUtility.TryParseHtmlString("#" + COLOREVEN, out color); }
        else { ColorUtility.TryParseHtmlString("#" + COLORODD, out color); }
        colourCount++;
        return color;
    }
}
