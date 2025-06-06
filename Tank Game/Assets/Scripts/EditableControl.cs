using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EditableControl : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI controlName, controlBind;
    private Settings.Setting setting;

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

    private void ResetControl()
    {
        Debug.Log($"Reset {setting.GetName()}");
        setting.ResetSetting();
        controlBind.text = setting.GetCurrentValue();
    }

    private void WaitForNewControlInput()
    {
        Debug.Log($"Waiting for input for {setting.GetName()}");
        Settings.Singleton.ShowChangeControl(setting, NewInputRecieved);
    }

    void NewInputRecieved()
    {
        Debug.Log("Input recieved...");
        Debug.Log($"{setting.GetName()} now set to {setting.GetCurrentValue()}");
        controlBind.text = setting.GetCurrentValue();
    }

    private void SetupColour(Color32 colour)
    {
        GetComponent<RawImage>().color = colour;
    }

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
        if (colourCount % 2 == 0) { ColorUtility.TryParseHtmlString("#" + COLOREVEN, out color); }
        else { ColorUtility.TryParseHtmlString("#" + COLORODD, out color); }
        colourCount++;
        return color;
    }
}
