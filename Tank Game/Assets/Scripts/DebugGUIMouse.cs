#if UNITY_EDITOR // Runs only in the UNITY EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Editor-only utility for inspecting UI elements under the mouse.
/// Shows hover/click information in the Console and selects clicked objects in the Hierarchy.
/// </summary>
[ExecuteAlways]
public class DebugGUIMouse : MonoBehaviour
{
    private PointerEventData pointerEventData;
    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;

    /// <summary>
    /// Attempts to find required UI components when the script is enabled in the editor.
    /// Logs warnings if components are missing.
    /// </summary>
    void OnEnable()
    {
        // Automatically find the Raycaster and EventSystem if not assigned
        raycaster = FindAnyObjectByType<GraphicRaycaster>();
        eventSystem = FindAnyObjectByType<EventSystem>();

        if (raycaster == null)
        {
            Debug.LogWarning("EDITOR_DEBUG_MOUSE_UI: No GraphicRaycaster found in the scene.");
        }

        if (eventSystem == null)
        {
            Debug.LogWarning("EDITOR_DEBUG_MOUSE_UI: No EventSystem found in the scene.");
        }
    }

    /// <summary>
    /// Checks for UI elements under the mouse cursor while in Play Mode.
    /// Logs hovered elements and selects clicked UI GameObjects in the Hierarchy.
    /// </summary>
    void Update()
    {
        // Exit if not playing or dependencies aren't present
        if (!Application.isPlaying || raycaster == null || eventSystem == null) { return; }

        // Prepare pointer event using current mouse position
        pointerEventData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerEventData, results);

        // Show the top UI element under the cursor
        if (results.Count > 0)
        {
            Debug.Log($"[Hovering] Top UI Element: {results[0].gameObject.name}");
        }

        // On left-click, log and select the top UI element (if any)
        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            if (results.Count > 0)
            {
                GameObject clicked = results[0].gameObject;
                Debug.Log($"[Click] Clicked UI Element: {results[0].gameObject.name}");

                // Highlight clicked UI object in the Hierarchy/Inspector
                Selection.activeGameObject = clicked;
            }
            else
            {
                Debug.Log("[Click] No UI element clicked.");
            }
        }
    }
}
#endif
