using System;
using UnityEngine;

[CreateAssetMenu]
public class BarrleType : ScriptableObject
{
	[Range(1f, 8f)] 
	public float radius = 1f;
	public float damage = 10;
	public Color color = Color.red;
}
