using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeCreator : MonoBehaviour
{
	[HideInInspector]
	public List<Vector3> wayPoints = new List<Vector3>();

	public float handleRadius = .5f;
}
