using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WaypointsTool : EditorWindow
{
	[MenuItem("Tools/Waypoints")]
	public static void OpenTheThing() => GetWindow<WaypointsTool>();

	public Vector3[] points = new Vector3[4];
	public GameObject aiGameobject;
	public bool aiIsActive = false;
	SerializedObject so;
	SerializedProperty propPoints;

	private void OnEnable()
	{
		so = new SerializedObject(this);
		propPoints = so.FindProperty("points");

		Selection.selectionChanged += Repaint;
		SceneView.duringSceneGui += DuringSceneGUI;
	}

	private void OnDisable()
	{
		Selection.selectionChanged -= Repaint;
		SceneView.duringSceneGui -= DuringSceneGUI;
	}

	private void OnGUI()
	{
		GUILayout.Label("AI Setup", EditorStyles.boldLabel);
		using (new GUILayout.VerticalScope(EditorStyles.helpBox))
		{
			aiGameobject = EditorGUILayout.ObjectField("AI Gameobject: ", aiGameobject, typeof(GameObject), true) as GameObject;

			using (new GUILayout.HorizontalScope())
			{
				if (GUILayout.Button("Get AI prefab"))
				{
					aiGameobject = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/AI.prefab") as GameObject;
				}
				if (GUILayout.Button("Instantiate AI in Scene"))
				{
					if(Instantiate(aiGameobject))
					{
						aiIsActive = true;
					}
				}
			}
		}
	}

	void DuringSceneGUI(SceneView sceneView)
	{
		//creates waypoints 
		so.Update();
		for (int i = 0; i < propPoints.arraySize; i++)
		{
			SerializedProperty prop = propPoints.GetArrayElementAtIndex(i);
			prop.vector3Value = Handles.PositionHandle(prop.vector3Value, Quaternion.identity);
		}
		so.ApplyModifiedProperties();

		//connects waypoints with lines
		for (int i = 0; i < propPoints.arraySize; i++)
		{
			Handles.DrawLine(points[i], points[(int)Mathf.Repeat(i + 1, points.Length)]);
		}
	}
}
