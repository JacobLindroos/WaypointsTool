using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class ExplosiveBarrles : MonoBehaviour
{
	static readonly int shPropColor = Shader.PropertyToID("_Color");
	public BarrleType type;
	private MaterialPropertyBlock mpb;

	public MaterialPropertyBlock Mpb
	{
		get
		{
			if (mpb == null)
			{
				mpb = new MaterialPropertyBlock();
			}
			return mpb;
		} 
	}

	private void OnDrawGizmos()
	{
		if (type == null)
		{
			return;
		}

		Handles.color = type.color;
		Handles.DrawWireDisc(transform.position, transform.up, type.radius);
		Handles.color = Color.white;
	}

	private void OnEnable()
	{
		TryApplyColor();
		ExplosiveBarrleManager.allTheBarrles.Add(this);
	}

	//everytime u modify a value 
	private void OnValidate()
	{
		TryApplyColor();
	}

	private void OnDisable()
	{
		ExplosiveBarrleManager.allTheBarrles.Remove(this);
	}

	public void TryApplyColor()
	{
		if (type == null)
		{
			return;
		}
		MeshRenderer rnd = GetComponent<MeshRenderer>();
		Mpb.SetColor(shPropColor, type.color);
		rnd.SetPropertyBlock(Mpb);
	}
}
