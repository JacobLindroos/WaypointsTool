using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class SnapperTool : EditorWindow
{
	public enum GridType
	{
		Cartesian,
		Polar
	}

	[MenuItem("Tools/Snapper")]
	public static void OpenTheThing() => GetWindow<SnapperTool>("Snapper");

	public GridType gridType = GridType.Cartesian;
	public int angularDivisions = 24;
	public float gridSize = 1f;
	const float TAU = 6.28318530718f;
	//private Vector3 gridPoint;
	//private int gridCells;

	SerializedObject so;
	SerializedProperty propGridSize;
	SerializedProperty propGridType;
	SerializedProperty propAngularDivisions;

	private void OnEnable()
	{
		so = new SerializedObject(this);
		propGridSize = so.FindProperty("gridSize");
		propGridType = so.FindProperty("gridType");
		propAngularDivisions = so.FindProperty("angularDivisions");

		//load n save config
		gridSize = EditorPrefs.GetFloat("SNAPPER_TOOL_gridSize", 1f);
		gridType = (GridType)EditorPrefs.GetInt("SNAPPER_TOOL_gridType", 0);
		angularDivisions = EditorPrefs.GetInt("SNAPPER_TOOL_angularDivisions", 24);

		Selection.selectionChanged += Repaint;
		SceneView.duringSceneGui += DuringSceneGUI;
	}

	private void OnDisable()
	{
		//save config
		EditorPrefs.GetFloat("SNAPPER_TOOL_gridSize", gridSize);
		EditorPrefs.GetInt("SNAPPER_TOOL_gridType", (int)gridType);
		EditorPrefs.GetInt("SNAPPER_TOOL_angularDivisions", angularDivisions);

		Selection.selectionChanged -= Repaint;
		SceneView.duringSceneGui -= DuringSceneGUI;
	}


	void DuringSceneGUI(SceneView sceneView)
	{
		#region Simple grid using gridcells 

		//for (int x = -gridCells; x < gridCells; x++)
		//{
		//	for (int z = -gridCells; z < gridCells; z++)
		//	{
		//		gridPoint = new Vector3(x, 0, z);
		//		gridPoint = gridPoint.RoundGridSizePosition(gridSizeSlider);
		//		Handles.DrawSphere(0, gridPoint, Quaternion.identity, .35f);
		//	}
		//}

		#endregion

		if (Event.current.type == EventType.Repaint)
		{
			Handles.zTest = CompareFunction.LessEqual;
			const float gridDrawExtent = 16;
			if (gridType == GridType.Cartesian)
			{
				DrawGridCartesian(gridDrawExtent);
			}
			else
			{
				DrawGridPolar(gridDrawExtent);
			}
		}
	}

	void DrawGridPolar(float gridDrawExtent)
	{
		int ringCount = Mathf.RoundToInt(gridDrawExtent / gridSize);
		float radiusOuter = (ringCount - 1) * gridSize;
		//radial grid (rings)
		for (int i = 1; i < ringCount; i++)
		{
			Handles.DrawWireDisc(Vector3.zero, Vector3.up, i * gridSize);
		}

		//angular grid (lines)
		for (int i = 0; i < angularDivisions; i++)
		{
			float t = i / (float)(angularDivisions);
			float angRad = t * TAU;
			float x = Mathf.Cos(angRad);
			float y = Mathf.Sin(angRad);
			Vector3 dir = new Vector3(x, 0f, y);
			Handles.DrawAAPolyLine(Vector3.zero, dir * radiusOuter);
		}
	}

	void DrawGridCartesian(float gridDrawExtent)
	{
		int lineCount = Mathf.RoundToInt((gridDrawExtent * 2) / gridSize);
		if (lineCount % 2 == 0)
		{
			lineCount++;
		}
		int halfLineCount = lineCount / 2;
		for (int i = 0; i < lineCount; i++)
		{
			float intOffset = i - halfLineCount;
			float xCoord = intOffset * gridSize;
			float zCoord0 = halfLineCount * gridSize;
			float zCoord1 = -halfLineCount * gridSize;
			Vector3 p0 = new Vector3(xCoord, 0f, zCoord0);
			Vector3 p1 = new Vector3(xCoord, 0f, zCoord1);
			Handles.DrawAAPolyLine(p0, p1);
			p0 = new Vector3(zCoord0, 0f, xCoord);
			p1 = new Vector3(zCoord1, 0f, xCoord);
			Handles.DrawAAPolyLine(p0, p1);
		}
	}

	private void OnGUI()
	{
		so.Update();
		EditorGUILayout.PropertyField(propGridType);
		EditorGUILayout.PropertyField(propGridSize);
		if (gridType == GridType.Polar)
		{
			EditorGUILayout.PropertyField(propAngularDivisions);
			propAngularDivisions.intValue = Mathf.Max(4, propAngularDivisions.intValue);

		}
		if(so.ApplyModifiedProperties())
		{
			SceneView.RepaintAll();
		}

		#region gridsize and gridcells stuff
		//using (new GUILayout.HorizontalScope())
		//{
		//	GUILayout.Label("Gridsize: ", GUILayout.Width(60));
		//	gridSizeSlider = EditorGUILayout.Slider(gridSizeSlider, 1, 20);
		//}

		//using (new GUILayout.HorizontalScope())
		//{
		//	GUILayout.Label("Gridcells: ", GUILayout.Width(60));
		//	gridCells = EditorGUILayout.IntSlider(gridCells, 10, 20);
		//}
		#endregion

		using (new EditorGUI.DisabledScope(Selection.gameObjects.Length == 0))
		if (GUILayout.Button("Snap Selection"))
		{
			SnapSelection();	
		}
	}

	void SnapSelection()
	{
		foreach (GameObject go in Selection.gameObjects)
		{
			Undo.RecordObject(go.transform, "snap objects");
			go.transform.position = GetSnappedPosition(go.transform.position);
		}
	}

	Vector3 GetSnappedPosition(Vector3 posOriginal)
	{
		if (gridType == GridType.Cartesian)
		{
			return posOriginal.Round(gridSize);
		}

		if (gridType == GridType.Polar)
		{
			Vector2 vec = new Vector2(posOriginal.x, posOriginal.z);
			float dist = vec.magnitude;
			float distSnapped = dist.Round(gridSize);

			float angRad = Mathf.Atan2(vec.y, vec.x);
			float angTurns = angRad / TAU;
			float angTurnsSnapped = angTurns.Round(1f / angularDivisions);
			float angRadSnapped = angTurnsSnapped * TAU;

			Vector2 dirSnapped = new Vector2(Mathf.Cos(angRadSnapped), Mathf.Sin(angRadSnapped));
			Vector2 snappedVec = dirSnapped * distSnapped;

			return new Vector3(snappedVec.x, posOriginal.y, snappedVec.y);
		}

		return default;
	}
}
