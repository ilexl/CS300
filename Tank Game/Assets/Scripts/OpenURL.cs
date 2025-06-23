using UnityEngine;

/// <summary>
/// Provides methods to open a URL in the default system browser.
/// Supports opening a predefined URL or one provided as a parameter.
/// </summary>
public class OpenURL : MonoBehaviour
{
    [SerializeField] string url;

    /// <summary>
    /// Opens the predefined URL if valid; otherwise logs a warning.
    /// </summary>
    public void OpenURLLink()
    {
        if(url == null || url == "")
        {
            Debug.LogWarning("URL is BLANK or NULL...");
            return;
        }
        Application.OpenURL(url);
    }

    /// <summary>
    /// Opens the given URL if valid; otherwise logs a warning.
    /// </summary>
    /// <param name="url">The URL string to open.</param>
    public void OpenURLLink(string url)
    {
        if (url == null || url == "")
        {
            Debug.LogWarning("URL is BLANK or NULL...");
            return;
        }
        Application.OpenURL(url);
    }
}
