using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles clickable links inside a TextMeshProUGUI component.
/// Detects clicks on links and performs an action (e.g., open a URL).
/// </summary>
public class LinkHandler : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI textMeshPro;

    /// <summary>
    /// Called when the user clicks on the text. Checks if a link was clicked,
    /// then handles that link (currently opens it as a URL).
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // Find if the click was on a link within the TextMeshPro text
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMeshPro, eventData.position, null);
        if (linkIndex != -1)
        {
            // Get info about the clicked link
            TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo[linkIndex];
            string linkID = linkInfo.GetLinkID();

            // Open the link URL (or handle link ID as needed)
            Debug.Log("Link clicked: " + linkID);
            Application.OpenURL(linkID);
        }
    }
}
