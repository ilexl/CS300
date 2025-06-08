#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

using Object = UnityEngine.Object;

/// <summary>
/// Allow users to access Playmode 'step by step' button even if MultiplayerPlaymode is active.
/// </summary>
public static class PlayModeStepButtonFix
{
    private static VisualElement _cachedToolbar;
    public static VisualElement Toolbar
    {
        get
        {
            if (_cachedToolbar == null)
            {
                FetchToolbar();
            }

            return _cachedToolbar;
        }
    }

    static PlayModeStepButtonFix()
    {
        return;
        EditorApplication.playModeStateChanged += OnPlaymodeChanged;
    }

    private static void OnPlaymodeChanged(PlayModeStateChange mode)
    {
        return;
        if (mode == PlayModeStateChange.EnteredPlayMode)
        {
            EditorApplication.delayCall += () => EnableStepPlaymodeButton();
        }
    }

    public static void EnableStepPlaymodeButton()
    {
        return;
        if (Toolbar == null)
            return;

        VisualElement playmodeButtons = Toolbar.Q<VisualElement>("PlayMode");

        // 'playmode-dropdown' is unique to MultiplayerPlaymode overlay.
        VisualElement multiplayerDropdown = playmodeButtons.Q<VisualElement>("playmode-dropdown");

        // MultiplayerPlaymode pacakge is not acticve.
        if (multiplayerDropdown == null)
            return;

        VisualElement stepButton = multiplayerDropdown.parent.Query<VisualElement>("Step");

        if (stepButton == null)
            return;

        if (stepButton.ClassListContains("unity-disabled"))
        {
            stepButton.RemoveFromClassList("unity-disabled");
        }

        stepButton.SetEnabled(true);
    }

    private static void FetchToolbar()
    {
        return;
        Assembly unityEditorAssembly = typeof(EditorWindow).Assembly;
        Type GUIViewType = unityEditorAssembly.GetType("UnityEditor.Toolbar");

        if (GUIViewType == null)
        {
            Debug.LogError("Could not load Toolbar type trhough reflection.");
            return;
        }

        Object[] allToolbars = UnityEngine.Resources.FindObjectsOfTypeAll(GUIViewType);

        if (allToolbars == null)
        {
            Debug.LogError("Could not find any 'Toolbar' instances.");
            return;
        }

        FieldInfo toolbarRootAccessor = GUIViewType.GetField("m_Root", BindingFlags.Instance | BindingFlags.NonPublic);
        if (toolbarRootAccessor == null)
        {
            Debug.LogError($"Could not access to 'm_Root' member of Toolbar type.");
            return;
        }

        VisualElement root = (VisualElement)toolbarRootAccessor.GetValue(allToolbars[0]);

        if (root == null)
        {
            Debug.LogError($"Could not access to Toolbar root object");
            return;
        }

        _cachedToolbar = root;
    }
}
#endif