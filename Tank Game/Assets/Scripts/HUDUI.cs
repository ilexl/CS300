using UnityEngine;

public class HUDUI : MonoBehaviour
{
    public static HUDUI current;
    [SerializeField] private WindowManager windowManager;
    [SerializeField] private Window sniperWindow;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        current = this;
        
    }

    private void Awake()
    {
        current = this;
    }

    public void ShowSniperMode()
    {
        if (windowManager == null || sniperWindow == null)
        {
            Debug.LogWarning("HUDUI is missing required variable assignments...");
            return;
        }
        windowManager.ShowOnly(sniperWindow);
    }

    public void HideSniperMode()
    {
        if (windowManager == null || sniperWindow == null)
        {
            Debug.LogWarning("HUDUI is missing required variable assignments...");
            return;
        }
        windowManager.HideOnly(sniperWindow);
    }
}
