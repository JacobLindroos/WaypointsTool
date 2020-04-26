using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class Snapper
{
	const string UNDO_STR_SNAP = "snap objects";

	[MenuItem("Edit/Snap Selected Objects %&S", isValidateFunction: true)]
	public static bool SnapTheThingsValidate() => Selection.gameObjects.Length > 0;

	[MenuItem("Edit/Snap Selected Object %&S")]
	public static void SnapTheThings()
	{
		foreach (GameObject go in Selection.gameObjects)
		{
			Undo.RecordObject(go.transform, UNDO_STR_SNAP);
			go.transform.position = go.transform.position.Round();
		}
	}

	//public static Vector3 Round(this Vector3 v)
	//{
	//	v.x = Mathf.Round(v.x);
	//	v.y = Mathf.Round(v.y);
	//	v.z = Mathf.Round(v.z);
	//	return v;
	//}

	//public static Vector3 Round(this Vector3 v, float size)
	//{
	//	return (v / size).Round() * size;
	//}

	//public static float Round(this float v, float size)
	//{
	//	return Mathf.Round(v / size) * size;
	//}
}
