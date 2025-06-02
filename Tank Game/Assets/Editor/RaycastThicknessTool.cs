using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class RaycastThicknessTool
{
    // static RaycastThicknessTool()
    // {
    //     SceneView.duringSceneGui += OnSceneGUI;
    // }

    private static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0) // Left-click
        {
            HandleRaycast(e.mousePosition);
            e.Use(); // Consume event
        }
    }

    private static void HandleRaycast(Vector2 mousePosition)
    {
        if (EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (EditorApplication.isUpdating) return; // Prevents execution during asset imports
        if (BuildPipeline.isBuildingPlayer) return; // Prevents issues during builds
        // Convert mouse position to world ray
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

        // Perform the first raycast to find the first hit (entry point)
        if (Physics.Raycast(ray, out RaycastHit firstHit))
        {
            Collider collider = firstHit.collider;
            Vector3 entryPoint = firstHit.point;
            Vector3 exitPoint = Vector3.zero;

            // Enable backface hits
            Physics.queriesHitBackfaces = true;
            
            // Cast a second ray from the entry point to find the exit point
            Ray secondRay = new Ray(entryPoint + ray.direction * 0.001f, ray.direction); // Start just beyond the entry point
            if (collider.Raycast(secondRay, out RaycastHit secondHit, Mathf.Infinity))
            {
                exitPoint = secondHit.point;
                float thickness = Vector3.Distance(entryPoint, exitPoint);

                Debug.Log($"Object: {collider.name}, Thickness: {thickness} units");

                // Draw debug lines in Scene view
                Debug.DrawLine(entryPoint, exitPoint, Color.green, 5);
            }
            else
            {
                Debug.Log($"Object: {collider.name}, but no exit detected.");
            }
        }
        else
        {
            Debug.Log("No object hit.");
        }
    }
}