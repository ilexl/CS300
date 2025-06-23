using UnityEngine;

#region using editor
#if UNITY_EDITOR
using UnityEditor;
#endif
#endregion

/// <summary>
/// Manages a collection of UI windows, providing functions to show, hide, or toggle them individually or collectively.
/// </summary>
public class WindowManager : MonoBehaviour
{
    [SerializeField] private Window[] Windows;
    [SerializeField] private bool startFeature = true;

    // Awake is called when the WM is loading
    private void Awake()
    {
        OnEnable();
    }

    // Start is called before the first frame
    public void Start()
    {
        if (!startFeature) { return; }
        foreach(Window window in Windows)
        {
            if (window.ShowOnStart)
            {
                window.Show();
            }
            else
            {
                window.Hide();
            }
        }
    }

    // OnEnable is called when the WM is turned on or loaded
    public void OnEnable()
    {
        Windows = new Window[] { };
        int windowsAmount = 0;
        foreach (Transform _transform in transform)
        {
            bool successfullyFoundChildWindow = _transform.TryGetComponent<Window>(out Window window);
            if (successfullyFoundChildWindow)
            {
                windowsAmount++;
            }
        }

        Windows = new Window[windowsAmount];
        int counter = 0;
        foreach (Transform _transform in transform)
        {
            bool successfullyFoundChildWindow = _transform.TryGetComponent<Window>(out Window window);
            if (successfullyFoundChildWindow)
            {
                Windows[counter++] = window;
            }
        }
    }

    // Show window functions (hides all others)

    /// <summary>
    /// Shows the individual window - Hides all other windows the WM manages
    /// </summary>
    /// <param sendName="window">The window to show</param>
    public void ShowWindow(Window window)
    {
        Debug.Log($"Showing window {window}");
        foreach(Window _window in Windows)
        {
            if(_window == window)
            {
                _window.Show(); // Shows the required window
            }
            else
            {
                _window.Hide(); // Hides all others
            }
        }
    }

    /// <summary>
    /// Shows the individual window - Hides all other windows the WM manages
    /// </summary>
    /// <param sendName="windowIndex">The index in windows to show</param>
    public void ShowWindow(int windowIndex)
    {
        if (windowIndex >= Windows.Length)
        {
#if UNITY_EDITOR
            Debug.LogError("Index out of range - " + windowIndex + " for Windows in WM...");
#endif
        }
        else
        {
            ShowWindow(Windows[windowIndex]);
            Debug.Log($"Showing window {Windows[windowIndex]}");
        }
    }

    /// <summary>
    /// Shows the individual window - Hides all other windows the WM manages
    /// </summary>
    /// <param sendName="windowName">The transform sendName of the window to show</param>
    public void ShowWindow(string windowName)
    {
        bool found = false;
        foreach(Window window in Windows)
        {
            if(window.GetName() == windowName)
            {
                Debug.Log($"Showing window {window}");
                ShowWindow(window);
                found = true;
            }
        }
        if (!found)
        {
#if UNITY_EDITOR
            Debug.LogWarning("Window not found - " + windowName);
#endif
        }
    }


    // Show window functions (leaves all others as current)

    /// <summary>
    /// Shows the individual window - Leaves all other windows the WM manages
    /// </summary>
    /// <param sendName="window">The window to show</param>
    public void ShowOnly(Window window)
    {
        Debug.Log($"Showing only window {window}");
        foreach (Window _window in Windows)
        {
            if (_window == window)
            {
                _window.Show(); // Shows the required window
            }
        }
    }

    /// <summary>
    /// Shows the individual window - Leaves all other windows the WM manages
    /// </summary>
    /// <param sendName="windowIndex">The index in windows to show</param>
    public void ShowOnly(int windowIndex)
    {
        if (windowIndex >= Windows.Length)
        {
#if UNITY_EDITOR
            Debug.LogError("Index out of range - " + windowIndex + " for Windows in WM...");
#endif
        }
        else
        {
            Debug.Log($"Showing only window {Windows[windowIndex]}");
            ShowOnly(Windows[windowIndex]);
        }
    }

    /// <summary>
    /// Shows the individual window - Leaves all other windows the WM manages
    /// </summary>
    /// <param sendName="windowName">The transform sendName of the window to show</param>
    public void ShowOnly(string windowName)
    {
        bool found = false;
        foreach (Window window in Windows)
        {
            if (window.GetName() == windowName)
            {
                Debug.Log($"Showing only window {window}");
                ShowOnly(window);
                found = true;
            }
        }
        if (!found)
        {
#if UNITY_EDITOR
            Debug.LogWarning("Window not found - " + windowName);
#endif
        }
    }

    // Hide All Windows Function

    /// <summary>
    /// Hides all the windows managed by the WM
    /// </summary>
    public void HideAll()
    {
        Debug.Log($"Hiding all windows");
        foreach (Window window in Windows)
        {
            window.Hide();
        }
    }

    // Show All Windows Function

    /// <summary>
    /// Shows all windows managed by the WM
    /// </summary>
    public void ShowAll()
    {
        Debug.Log($"Showing all windows");
        foreach (Window window in Windows)
        {
            window.Show();
        }
    }

    // Hide Specific Window (Individually Hide)

    /// <summary>
    /// Hides the individual window - Leaves all other windows the WM manages
    /// </summary>
    /// <param sendName="window">The window to hide</param>
    public void HideOnly(Window window)
    {
        Debug.Log($"Hiding only window {window}");

        foreach (Window _window in Windows)
        {
            if (_window == window)
            {
                _window.Hide(); // Shows the required window
            }
        }
    }

    /// <summary>
    /// Hides the individual window - Leaves all other windows the WM manages
    /// </summary>
    /// <param sendName="windowIndex">The index in windows to Hide</param>
    public void HideOnly(int windowIndex)
    {
        if (windowIndex >= Windows.Length)
        {
#if UNITY_EDITOR
            Debug.LogError("Index out of range - " + windowIndex + " for Windows in WM...");
#endif
        }
        else
        {
            Debug.Log($"Hiding only window {Windows[windowIndex]}");
            HideOnly(Windows[windowIndex]);
        }
    }

    /// <summary>
    /// Hides the individual window - Leaves all other windows the WM manages
    /// </summary>
    /// <param sendName="windowName">The transform sendName of the window to Hide</param>
    public void HideOnly(string windowName)
    {
        bool found = false;
        foreach (Window window in Windows)
        {
            if (window.GetName() == windowName)
            {
                Debug.Log($"Hiding only window {window}");
                HideOnly(window);
                found = true;
            }
        }
        if (!found)
        {
#if UNITY_EDITOR
            Debug.LogWarning("Window not found - " + windowName);
#endif
        }
    }

    /// <summary>
    /// Gets all the windows as an array (readonly)
    /// </summary>
    /// <returns>WM windows</returns>
    public Window[] GetWindows()
    {
        return Windows;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        OnEnable();
    }
#endif
}

// *******************************************
// custom editor below - wont be in any builds
// *******************************************

#if UNITY_EDITOR
[CustomEditor(typeof(WindowManager))]
public class EDITOR_WindowManager : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        WindowManager WM = (WindowManager)target;
        Window[] Windows = WM.GetWindows();

        if (GUILayout.Button("Get windows"))
        {
            WM.OnEnable();
        }
        if (GUILayout.Button("Show ALL windows"))
        {
            WM.ShowAll();
        }

        if (GUILayout.Button("Hide ALL windows"))
        {
            WM.HideAll();
        }
        if (GUILayout.Button("Test Start"))
        {
            WM.Start();
        }

        foreach (Window window in Windows)
        {
            GUILayout.Space(10);
            if (GUILayout.Button("Show Window - " + window.GetName() + " - (Hides Others)")) WM.ShowWindow(window);
            if (GUILayout.Button("Show Only - " + window.GetName() + " - (Leaves Others)")) WM.ShowOnly(window);
            if (GUILayout.Button("Hide Only - " + window.GetName() + " - (Leaves Others)")) WM.HideOnly(window);
            GUILayout.Space(10);

        }
    }
}
#endif