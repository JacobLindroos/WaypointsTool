using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AIMove : MonoBehaviour
{
	[SerializeField] ShapeCreator waypointCreator;
	public float speed;

	private int current;

	private void Update()
	{
		if (transform.position != new Vector3(waypointCreator.wayPoints[current].x, waypointCreator.wayPoints[current].y, waypointCreator.wayPoints[current].z))
		{
			Vector3 position = Vector3.MoveTowards(transform.position, new Vector3(waypointCreator.wayPoints[current].x, waypointCreator.wayPoints[current].y, waypointCreator.wayPoints[current].z), speed * Time.deltaTime);
			GetComponent<Rigidbody>().MovePosition(position);
		}

		else current = (current + 1) % waypointCreator.wayPoints.Count;
	}
}
