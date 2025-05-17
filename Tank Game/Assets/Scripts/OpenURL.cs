using UnityEngine;

public class OpenURL : MonoBehaviour
{
    [SerializeField] string url;
    public void OpenURLLink()
    {
        if(url == null || url == "")
        {
            Debug.LogWarning("URL is BLANK or NULL...");
            return;
        }
        Application.OpenURL(url);
    }

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
