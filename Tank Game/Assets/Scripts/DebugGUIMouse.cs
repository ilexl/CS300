#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteAlways]
public class DebugGUIMouse : MonoBehaviour
{
    private PointerEventData pointerEventData;
    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;

    void OnEnable()
    {
        // Automatically find the Raycaster and EventSystem if not assigned
        raycaster = FindAnyObjectByType<GraphicRaycaster>();
        eventSystem = FindAnyObjectByType<EventSystem>();

        if (raycaster == null)
            Debug.LogWarning("EDITOR_DEBUG_MOUSE_UI: No GraphicRaycaster found in the scene.");

        if (eventSystem == null)
            Debug.LogWarning("EDITOR_DEBUG_MOUSE_UI: No EventSystem found in the scene.");
    }

    void Update()
    {
        if (!Application.isPlaying || raycaster == null || eventSystem == null)
            return;

        pointerEventData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerEventData, results);

        if (results.Count > 0)
        {
            Debug.Log($"[Hovering] Top UI Element: {results[0].gameObject.name}");
        }

        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            if (results.Count > 0)
            {
                GameObject clicked = results[0].gameObject;
                Debug.Log($"[Click] Clicked UI Element: {results[0].gameObject.name}");

                // Highlight in Inspector
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
