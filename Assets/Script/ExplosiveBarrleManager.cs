using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ExplosiveBarrleManager : MonoBehaviour
{
	public static List<ExplosiveBarrles> allTheBarrles = new List<ExplosiveBarrles>();

	public static void UpdateAllBarrelColors()
	{
		foreach (ExplosiveBarrles barrel in allTheBarrles)
		{
			barrel.TryApplyColor();
		} 
	}

	#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		Handles.zTest = CompareFunction.LessEqual;

		foreach (ExplosiveBarrles barrle in allTheBarrles)
		{
			if (barrle.type == null)
			{
				continue;
			}

			Vector3 managerPos = transform.position;
			Vector3 barrlePos = barrle.transform.position;
			float halftHeight = (managerPos.y - barrlePos.y) * .5f;
			Vector3 tangentOffset = Vector3.up * halftHeight;

			Handles.DrawBezier(managerPos, barrlePos, managerPos - tangentOffset, barrlePos + tangentOffset, barrle.type.color, EditorGUIUtility.whiteTexture, 1f);
			//Handles.DrawAAPolyLine(transform.position, barrle.transform.position);
			//Gizmos.DrawLine(transform.position, barrle.transform.position);
		}
	}
	#endif
}
