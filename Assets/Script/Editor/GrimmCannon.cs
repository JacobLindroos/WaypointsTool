//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEditor;
//using UnityEngine;
//using UnityEngine.Rendering;
//using Random = UnityEngine.Random;

//public struct SpawnData
//{
//	public Vector2 pointInDisc;
//	public float randAngleDeg;
//	public GameObject prefab;

//	public void SetRandomValues(List<GameObject> prefabs)
//	{
//		pointInDisc = Random.insideUnitCircle;
//		randAngleDeg = Random.value * 360;
//		prefab = prefabs.Count == 0 ? null : prefabs[Random.Range(0, prefabs.Count)];
//	}


//}

//public class SpawnPoint
//{

//	public SpawnData spawnData;
//	public Vector3 position;
//	public Quaternion rotation;
//	public bool isValid = false;

//	public Vector3 Up => rotation * Vector3.up;

//	public SpawnPoint(Vector3 position, Quaternion rotation, SpawnData spawnData)
//	{
//		this.spawnData = spawnData;
//		this.position = position;
//		this.rotation = rotation;

//		// check if this mesh can be placed/fit the current location
//		if (spawnData.prefab != null)
//		{
//			SpawnablePrefab spawnablePrefab = spawnData.prefab.GetComponent<SpawnablePrefab>();
//			if (spawnablePrefab == null)
//			{
//				isValid = true;
//			}
//			else
//			{
//				float h = spawnablePrefab.height;
//				Ray ray = new Ray(position, Up);
//				isValid = Physics.Raycast(ray, h) == false;
//			}
//		}
//	}


//}

//public class GrimmCannon : EditorWindow
//{

//	[MenuItem("Tools/Grimm Cannon")]
//	public static void OpenGrimm() => GetWindow<GrimmCannon>();

//	public float radius = 2f;
//	public int spawnCount = 8;

//	SerializedObject so;
//	SerializedProperty propRadius;
//	SerializedProperty propSpawnCount;
//	SerializedProperty propSpawnPrefab;
//	SerializedProperty propPreviewMaterial;

//	SpawnData[] spawnDataPoints;
//	GameObject[] prefabs;
//	List<GameObject> spawnPrefabs = new List<GameObject>();

//	Material materialInvalid;

//	[SerializeField] bool[] prefabSelectionStates;

//	void OnEnable()
//	{
//		so = new SerializedObject(this);
//		propRadius = so.FindProperty("radius");
//		propSpawnCount = so.FindProperty("spawnCount");
//		GenerateRandomPoints();
//		SceneView.duringSceneGui += DuringSceneGUI;

//		Shader sh = Shader.Find("Unlit/InvalidSpawn");
//		materialInvalid = new Material(sh);

//		// load prefabs
//		string[] guids = AssetDatabase.FindAssets("t:prefab", new[] { "Assets/Prefabs" });
//		IEnumerable<string> paths = guids.Select(AssetDatabase.GUIDToAssetPath);
//		prefabs = paths.Select(AssetDatabase.LoadAssetAtPath<GameObject>).ToArray();
//		if (prefabSelectionStates == null || prefabSelectionStates.Length != prefabs.Length)
//			prefabSelectionStates = new bool[prefabs.Length];
//	}

//	void OnDisable()
//	{
//		SceneView.duringSceneGui -= DuringSceneGUI;
//		DestroyImmediate(materialInvalid);
//	}


//	void GenerateRandomPoints()
//	{
//		spawnDataPoints = new SpawnData[spawnCount];
//		for (int i = 0; i < spawnCount; i++)
//			spawnDataPoints[i].SetRandomValues(spawnPrefabs);
//	}


//	void OnGUI()
//	{
//		so.Update();
//		EditorGUILayout.PropertyField(propRadius);
//		propRadius.floatValue = propRadius.floatValue.AtLeast(1);
//		EditorGUILayout.PropertyField(propSpawnCount);
//		propSpawnCount.intValue = propSpawnCount.intValue.AtLeast(1);

//		if (so.ApplyModifiedProperties())
//		{
//			GenerateRandomPoints();
//			SceneView.RepaintAll();
//		}

//		// if you clicked left mouse button, in the editor window
//		if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
//		{
//			GUI.FocusControl(null);
//			Repaint(); // repaint on the editor window UI
//		}
//	}


//	void TrySpawnObjects(List<SpawnPoint> spawnPoints)
//	{
//		if (spawnPrefabs.Count == 0)
//			return;
//		foreach (SpawnPoint spawnPoint in spawnPoints)
//		{
//			if (spawnPoint.isValid == false)
//				continue;

//			// spawn prefab
//			GameObject spawnedThing = (GameObject)PrefabUtility.InstantiatePrefab(spawnPoint.spawnData.prefab);
//			Undo.RegisterCreatedObjectUndo(spawnedThing, "Spawn Objects");
//			spawnedThing.transform.position = spawnPoint.position;
//			spawnedThing.transform.rotation = spawnPoint.rotation;
//		}

//		GenerateRandomPoints(); // update points
//	}


//	bool TryRaycastFromCamera(Vector2 cameraUp, out Matrix4x4 tangentToWorldMtx)
//	{
//		Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
//		if (Physics.Raycast(ray, out RaycastHit hit))
//		{
//			// setting up tangent space
//			Vector3 hitNormal = hit.normal;
//			Vector3 hitTangent = Vector3.Cross(hitNormal, cameraUp).normalized;
//			Vector3 hitBitangent = Vector3.Cross(hitNormal, hitTangent);
//			tangentToWorldMtx = Matrix4x4.TRS(hit.point, Quaternion.LookRotation(hitNormal, hitBitangent), Vector3.one);
//			return true;
//		}

//		tangentToWorldMtx = default;
//		return false;
//	}


//	void DuringSceneGUI(SceneView sceneView)
//	{
//		// button on top of the scene view to select prefabs
//		Handles.BeginGUI();
//		Rect rect = new Rect(8, 8, 64, 64);
//		for (int i = 0; i < prefabs.Length; i++)
//		{
//			GameObject prefab = prefabs[i];
//			Texture icon = AssetPreview.GetAssetPreview(prefab);

//			EditorGUI.BeginChangeCheck();
//			prefabSelectionStates[i] = GUI.Toggle(rect, prefabSelectionStates[i], new GUIContent(icon));
//			if (EditorGUI.EndChangeCheck())
//			{
//				// update selection list
//				spawnPrefabs.Clear();
//				for (int j = 0; j < prefabs.Length; j++)
//				{
//					if (prefabSelectionStates[j])
//						spawnPrefabs.Add(prefabs[j]);
//				}

//				GenerateRandomPoints();
//			}


//			//spawnPrefabs = prefab;
//			rect.y += rect.height + 2;
//		}

//		Handles.EndGUI();

//		Handles.zTest = CompareFunction.LessEqual;
//		Transform camTf = sceneView.camera.transform;

//		// make sure it repaints on mouse move
//		if (Event.current.type == EventType.MouseMove)
//		{
//			sceneView.Repaint();
//		}


//		// change radius
//		bool holdingAlt = (Event.current.modifiers & EventModifiers.Alt) != 0;
//		if (Event.current.type == EventType.ScrollWheel && holdingAlt == false)
//		{
//			float scrollDir = Mathf.Sign(Event.current.delta.y);
//			so.Update();
//			propRadius.floatValue *= 1f + scrollDir * 0.05f;
//			so.ApplyModifiedProperties();
//			Repaint(); // updates editor window
//			Event.current.Use(); // consume the event, don't let it fall through
//		}


//		// if the cursor is pointing on valid ground
//		if (TryRaycastFromCamera(camTf.up, out Matrix4x4 tangentToWorld))
//		{
//			List<SpawnPoint> spawnPoints = GetSpawnPoints(tangentToWorld);

//			if (Event.current.type == EventType.Repaint)
//			{
//				DrawCircleRegion(tangentToWorld);
//				DrawSpawnPreviews(spawnPoints, sceneView.camera);
//			}

//			// spawn on press
//			if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space)
//				TrySpawnObjects(spawnPoints);
//		}
//	}

//	void DrawSpawnPreviews(List<SpawnPoint> spawnPoints, Camera cam)
//	{
//		foreach (SpawnPoint spawnPoint in spawnPoints)
//		{
//			if (spawnPoint.spawnData.prefab != null)
//			{
//				// draw preview of all meshes in the prefab
//				Matrix4x4 poseToWorld = Matrix4x4.TRS(spawnPoint.position, spawnPoint.rotation, Vector3.one);
//				DrawPrefab(spawnPoint.spawnData.prefab, poseToWorld, cam, spawnPoint.isValid);
//			}
//			else
//			{
//				// prefab missing, draw sphere and normal on surface instead
//				Handles.SphereHandleCap(-1, spawnPoint.position, Quaternion.identity, 0.1f, EventType.Repaint);
//				Handles.DrawAAPolyLine(spawnPoint.position, spawnPoint.position + spawnPoint.Up);
//			}
//		}
//	}

//	void DrawPrefab(GameObject prefab, Matrix4x4 poseToWorld, Camera cam, bool valid)
//	{
//		MeshFilter[] filters = prefab.GetComponentsInChildren<MeshFilter>();
//		foreach (MeshFilter filter in filters)
//		{
//			Matrix4x4 childToPose = filter.transform.localToWorldMatrix;
//			Matrix4x4 childToWorldMtx = poseToWorld * childToPose;
//			Mesh mesh = filter.sharedMesh;
//			Material mat = valid ? filter.GetComponent<MeshRenderer>().sharedMaterial : materialInvalid;
//			Graphics.DrawMesh(mesh, childToWorldMtx, mat, 0, cam);
//		}
//	}

//	List<SpawnPoint> GetSpawnPoints(Matrix4x4 tangentToWorld)
//	{
//		List<SpawnPoint> hitSpawnPoints = new List<SpawnPoint>();
//		foreach (SpawnData rndDataPoint in spawnDataPoints)
//		{
//			// create ray for this point
//			Ray ptRay = GetCircleRay(tangentToWorld, rndDataPoint.pointInDisc);
//			// raycast to find point on surface
//			if (Physics.Raycast(ptRay, out RaycastHit ptHit))
//			{
//				// calculate rotation and assign to pose together with position
//				Quaternion randRot = Quaternion.Euler(0f, 0f, rndDataPoint.randAngleDeg);
//				Quaternion rot = Quaternion.LookRotation(ptHit.normal) * (randRot * Quaternion.Euler(90f, 0f, 0f));
//				SpawnPoint spawnPoint = new SpawnPoint(ptHit.point, rot, rndDataPoint);
//				hitSpawnPoints.Add(spawnPoint);
//			}
//		}

//		return hitSpawnPoints;
//	}

//	Ray GetCircleRay(Matrix4x4 tangentToWorld, Vector2 pointInCircle)
//	{
//		Vector3 normal = tangentToWorld.MultiplyVector(Vector3.forward);
//		Vector3 rayOrigin = tangentToWorld.MultiplyPoint3x4(pointInCircle * radius);
//		rayOrigin += normal * 2; // offset margin thing
//		Vector3 rayDirection = -normal;
//		return new Ray(rayOrigin, rayDirection);
//	}

//	void DrawCircleRegion(Matrix4x4 localToWorld)
//	{
//		DrawAxes(localToWorld);
//		// draw circle adapted to the terrain
//		const int circleDetail = 128;
//		Vector3[] ringPoints = new Vector3[circleDetail];
//		for (int i = 0; i < circleDetail; i++)
//		{
//			float t = i / ((float)circleDetail - 1); // go back to 0/1 position
//			const float TAU = 6.28318530718f;
//			float angRad = t * TAU;
//			Vector2 dir = new Vector2(Mathf.Cos(angRad), Mathf.Sin(angRad));
//			Ray r = GetCircleRay(localToWorld, dir);
//			if (Physics.Raycast(r, out RaycastHit cHit))
//			{
//				ringPoints[i] = cHit.point + cHit.normal * 0.02f;
//			}
//			else
//			{
//				ringPoints[i] = r.origin;
//			}
//		}

//		Handles.DrawAAPolyLine(ringPoints);
//	}

//	void DrawAxes(Matrix4x4 localToWorld)
//	{
//		Vector3 pt = localToWorld.MultiplyPoint3x4(Vector3.zero);
//		Handles.color = Color.red;
//		Handles.DrawAAPolyLine(6, pt, pt + localToWorld.MultiplyVector(Vector3.right));
//		Handles.color = Color.green;
//		Handles.DrawAAPolyLine(6, pt, pt + localToWorld.MultiplyVector(Vector3.up));
//		Handles.color = Color.blue;
//		Handles.DrawAAPolyLine(6, pt, pt + localToWorld.MultiplyVector(Vector3.forward));
//	}


//}