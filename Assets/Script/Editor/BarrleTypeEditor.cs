using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(BarrleType))]
public class BarrleTypeEditor : Editor
{

	//public float thing; //serialized, visible, public
	//private float thing1; //not serialized, hidden, private
	//[SerializeField] private float thing2; //serialized, visible, private
	//[HideInInspector] public float thing3; //serialized, hidden, public

	SerializedObject so;
	SerializedProperty propRadius;
	SerializedProperty propDamage;
	SerializedProperty propColor;

	private void OnEnable()
	{
		so = serializedObject;
		propRadius = so.FindProperty("radius");
		propDamage = so.FindProperty("damage");
		propColor = so.FindProperty("color");
	}

	public override void OnInspectorGUI()
	{
		so.Update();
		EditorGUILayout.PropertyField(propRadius);
		EditorGUILayout.PropertyField(propDamage);
		EditorGUILayout.PropertyField(propColor);
		if (so.ApplyModifiedProperties())
		{
			ExplosiveBarrleManager.UpdateAllBarrelColors();
		}

		//explicit positioning using Rect
		//GUI
		//EditorGUI

		//implicit positioning, auto-layout
		//EditorGUILayout
		//GUILayout

	}
}
