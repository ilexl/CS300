using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI component representing an editable control binding in settings.
/// Allows left-click to change the binding and right-click to reset it.
/// Updates UI text and color based on the associated setting.
/// </summary>
public class EditableControl : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI controlName, controlBind;
    private Settings.Setting setting;

    /// <summary>
    /// Handles mouse clicks on the control UI element.
    /// Left-click initiates re-binding input process.
    /// Right-click resets the binding to default.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            WaitForNewControlInput(); // Mouse 0 (left click)
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            ResetControl(); // Mouse 1 (right click)
        }
    }

    /// <summary>
    /// Resets the associated setting to its default value and updates the UI text accordingly.
    /// </summary>
    private void ResetControl()
    {
        Debug.Log($"Reset {setting.GetName()}");
        setting.ResetSetting();
        controlBind.text = setting.GetCurrentValue();
    }

    /// <summary>
    /// Initiates the UI flow to wait for new input from the user for rebinding this control.
    /// </summary>
    private void WaitForNewControlInput()
    {
        Debug.Log($"Waiting for input for {setting.GetName()}");
        Settings.Singleton.ShowChangeControl(setting, NewInputRecieved);
    }

    /// <summary>
    /// Callback invoked when a new input binding is received.
    /// Updates the UI text to reflect the new binding.
    /// </summary>
    void NewInputRecieved()
    {
        Debug.Log("Input recieved...");
        Debug.Log($"{setting.GetName()} now set to {setting.GetCurrentValue()}");
        controlBind.text = setting.GetCurrentValue();
    }

    /// <summary>
    /// Applies the background color to the RawImage component for alternating row coloring.
    /// </summary>
    private void SetupColour(Color32 colour)
    {
        GetComponent<RawImage>().color = colour;
    }

    /// <summary>
    /// Initializes the control UI element with the specified setting.
    /// Formats the control name for readability and sets the initial binding text and color.
    /// </summary>
    public void Setup(Settings.Setting setting)
    {
        this.setting = setting;
        Debug.Log($"Setting up {setting.GetName()}");
        SetupColour(GetColour());

        if (controlName != null)
        {
            controlName.text = Regex.Replace(setting.GetName().Split('-')[1], "(?<=[a-z])([A-Z])", " $1");
        }

        if (controlBind != null)
        {
            controlBind.text = setting.GetCurrentValue();
        }
    }

    // Colors for alternating row backgrounds to improve readability in UI lists
    const string COLORODD = "3D3D3D";
    const string COLOREVEN = "4D4D4D";

    // Keeps track of how many controls have been setup for color alternation
    static int colourCount = 0;

    /// <summary>
    /// Resets the static color alternation counter.
    /// Useful when refreshing the entire controls list to maintain consistent striping.
    /// </summary>
    public static void ResetColourCount()
    {
        colourCount = 0;
    }

    /// <summary>
    /// Returns the alternating color for control background based on setup order.
    /// Even-indexed controls get one color, odd-indexed another.
    /// </summary>
    public static Color GetColour()
    {
        Color color;
        if (colourCount % 2 == 0) { ColorUtility.TryParseHtmlString("#" + COLOREVEN, out color); }
        else { ColorUtility.TryParseHtmlString("#" + COLORODD, out color); }
        colourCount++;
        return color;
    }
}
