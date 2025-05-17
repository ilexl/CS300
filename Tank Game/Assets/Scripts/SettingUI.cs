using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        GetComponent<RawImage>().color = GetColour(); // alternating backgrounds
    }

    void GetCurrentSelection()
    {
        if(setting.GetCurrentValue() == "HIGHEST")
        {
            // it will be the very last in the list (in theory)
            // TODO: test if highest values ARE the last in their options lists...
            currentSelection = options.Count - 1;
            setting.UpdateCurrentValue(options[currentSelection]);
            setting.SaveSetting(); // save setting to use known resolution of user
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

    string CleanAndSplitCamelCase(string input)
    {
        // Step 1: Remove everything before and including the first dash
        int dashIndex = input.IndexOf('-');
        if (dashIndex != -1)
        {
            input = input.Substring(dashIndex + 1);
        }

        // Step 2: If input has only one uppercase letter or all caps are adjacent, don't split
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

        // If all caps are grouped or only 1 capital letter, return unchanged
        if (uppercaseCount <= 1 || maxConsecutiveUppercase > 1)
        {
            return input;
        }

        // Step 3: Split on uppercase letters that are not the first character
        return Regex.Replace(input, "(?<!^)([A-Z])", " $1");
    }

    const string COLORODD = "3D3D3D";
    const string COLOREVEN = "4D4D4D";
    static int colourCount = 0;
    public static void ResetColourCount()
    {
        colourCount = 0;
    }
    public static Color GetColour()
    {
        Color color;
        if(colourCount % 2 == 0) { ColorUtility.TryParseHtmlString("#" + COLOREVEN, out color); }
        else { ColorUtility.TryParseHtmlString("#" + COLORODD, out color); }
        colourCount++;
        return color;
    }
}
