using UnityEngine;
using System.Collections;
using UnityEditor;
using Unity.VisualScripting;

[CustomEditor(typeof(Transform))]
public class SkeletonVisualizer : EditorWindow
{
	private bool isDrawing = false;

	// Add menu named "Draw Lines To Children" to the Window menu
	private GameObject selectedObject;

	// Add menu named "Draw Lines From Children" to the Window menu
	[MenuItem("Tools/Skeleton Visualizer")]
	public static void Init()
	{
		// Get existing open window or if none, make a new one:
		SkeletonVisualizer window = (SkeletonVisualizer)EditorWindow.GetWindow(typeof(SkeletonVisualizer));
		window.Show();
	}

	void OnGUI()
	{
		GUILayout.Label("Skeleton Visualizer", EditorStyles.boldLabel);
		isDrawing = EditorGUILayout.Toggle("Drawing Enabled", isDrawing);

		if (GUILayout.Button("Start Drawing"))
		{
			StartDrawing();
		}

		if (GUILayout.Button("Stop Drawing"))
		{
			StopDrawing();
		}
	}

	void OnSelectionChange()
	{
		// Update selected object when the selection changes
		if (Selection.activeGameObject != null)
		{
			selectedObject = Selection.activeGameObject;
			Repaint();
		}
	}

	void StartDrawing()
	{
		isDrawing = true;
		SceneView.duringSceneGui += OnSceneGUI;
	}

	void StopDrawing()
	{
		isDrawing = false;
		SceneView.duringSceneGui -= OnSceneGUI;
	}

	void OnSceneGUI(SceneView sceneView)
	{
		if (!isDrawing || selectedObject == null) return;

		if (selectedObject.transform.parent != null)
			Handles.DrawLine(selectedObject.transform.position, selectedObject.transform.parent.position);
		DrawLinesForSelectedObject(selectedObject.transform);
	}

	void DrawLinesForSelectedObject(Transform parent)
	{
		foreach (Transform child in parent)
		{
			Handles.DrawLine(child.position, parent.position);
			DrawLinesForSelectedObject(child); // Recursively draw for subchildren
		}
	}
}
