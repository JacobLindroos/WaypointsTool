using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShapeCreator))]
public class ShapeEditor : Editor
{
	ShapeCreator shapeCreator;
	SelectionInfo selectionInfo;
	bool needsRepaint;

	private void OnEnable()
	{
		shapeCreator = target as ShapeCreator;
		selectionInfo = new SelectionInfo();
	}

	//inputs into the scene and also layout and repaint event
	private void OnSceneGUI()
	{
		Event guiEvent = Event.current;

		if (guiEvent.type == EventType.Repaint)
		{
			Draw();
		}
		else if (guiEvent.type == EventType.Layout)
		{
			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
		}
		else
		{
			HandleInput(guiEvent);

			if (needsRepaint)
			{
				HandleUtility.Repaint();
			}
		}
	}

	void HandleInput(Event guiEvent)
	{
		//calculates the mouseclick position in the world coordinates
		Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
		float drawPlaneHeight = 0;
		float dstToDrawPlane = (drawPlaneHeight - mouseRay.origin.y) / mouseRay.direction.y;
		Vector3 mousePosition = mouseRay.GetPoint(dstToDrawPlane);

		//adds a waypoint on left mouse click
		if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.Shift)
		{
			HandleShiftLeftMouseDown(mousePosition);
		}

		if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
		{
			HandleLeftMouseDown(mousePosition);
		}

		if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0)
		{
			HandleLeftMouseUp(mousePosition);
		}

		if (guiEvent.type == EventType.MouseDrag && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
		{
			HandleLeftMouseDrag(mousePosition);
		}

		if (!selectionInfo.pointIsSelected)
		{
			UpdateMouseOverInfo(mousePosition);
		}
		
	}

	void Draw()
	{
		//drawing out disc at mouseclick
		for (int i = 0; i < shapeCreator.wayPoints.Count; i++)
		{
			//gets the next point
			Vector3 nextPoint = shapeCreator.wayPoints[(i + 1) % shapeCreator.wayPoints.Count];
			if (i == selectionInfo.lineIndex)
			{
				Handles.color = Color.red;
				//draws a line between the points
				Handles.DrawLine(shapeCreator.wayPoints[i], nextPoint);
			}
			else
			{
				Handles.color = Color.black;
				//draws a line between the points
				Handles.DrawDottedLine(shapeCreator.wayPoints[i], nextPoint, 4f);
			}

			//when mouse
			if (i == selectionInfo.pointIndex)
			{
				//Handles.color = Color.red;
				Handles.color = (selectionInfo.pointIsSelected) ? Color.black : Color.red;
			}
			else
			{
				Handles.color = Color.white;
			}
			//draws a disc representing the point
			Handles.DrawSolidDisc(shapeCreator.wayPoints[i], Vector3.up, shapeCreator.handleRadius);
		}

		needsRepaint = false;
	}

	void HandleLeftMouseDown(Vector3 mousePosition)
	{
		if (!selectionInfo.mouseIsOverPoint)
		{
			int newPointIndex = (selectionInfo.mouseIsOverLine) ? selectionInfo.lineIndex + 1 : shapeCreator.wayPoints.Count;
			Undo.RecordObject(shapeCreator, "Add point");
			shapeCreator.wayPoints.Insert(newPointIndex, mousePosition);
			selectionInfo.pointIndex = newPointIndex;
		}

		selectionInfo.pointIsSelected = true;
		selectionInfo.positionAtStartOfDrag = mousePosition;
		needsRepaint = true;
	}

	void HandleLeftMouseUp(Vector3 mousePosition)
	{
		if (selectionInfo.pointIsSelected)
		{
			shapeCreator.wayPoints[selectionInfo.pointIndex] = selectionInfo.positionAtStartOfDrag;
			Undo.RecordObject(shapeCreator, "Move point");
			shapeCreator.wayPoints[selectionInfo.pointIndex] = mousePosition;

			selectionInfo.pointIsSelected = false;
			selectionInfo.pointIndex = -1;
			needsRepaint = true;
		}
	}

	void HandleLeftMouseDrag(Vector3 mousePosition)
	{
		if (selectionInfo.pointIsSelected)
		{
			shapeCreator.wayPoints[selectionInfo.pointIndex] = mousePosition;
			needsRepaint = true;
		}
	}

	void DeletePointUnderMouse()
	{
		Undo.RecordObject(shapeCreator, "Delete point");
		shapeCreator.wayPoints.RemoveAt(selectionInfo.pointIndex);
		selectionInfo.pointIsSelected = false;
		selectionInfo.mouseIsOverPoint = false;
		needsRepaint = true;
	}

	void HandleShiftLeftMouseDown(Vector3 mousePosition)
	{
		if (selectionInfo.mouseIsOverPoint)
		{
			DeletePointUnderMouse();
		}
	}

	void UpdateMouseOverInfo(Vector3 mousePosition)
	{
		int mouseOverPointIndex = -1;
		for (int i = 0; i < shapeCreator.wayPoints.Count; i++)
		{
			//checking distance between mouse pos to waypoint
			if (Vector3.Distance(mousePosition, shapeCreator.wayPoints[i]) < shapeCreator.handleRadius)
			{
				mouseOverPointIndex = i;
				break;
			}
		}

		if (mouseOverPointIndex != selectionInfo.pointIndex)
		{
			selectionInfo.pointIndex = mouseOverPointIndex;
			selectionInfo.mouseIsOverPoint = mouseOverPointIndex != -1;

			needsRepaint = true;
		}

		if (selectionInfo.mouseIsOverPoint)
		{
			selectionInfo.mouseIsOverLine = false;
			selectionInfo.lineIndex = -1;
		}
		else
		{
			int mouseOverLineIndex = -1;
			float closestLineDst = shapeCreator.handleRadius;
			for (int i = 0; i < shapeCreator.wayPoints.Count; i++)
			{
				Vector3 nextPointInShape = shapeCreator.wayPoints[(i + 1) % shapeCreator.wayPoints.Count];
				float dstFromMouseToLine = HandleUtility.DistancePointToLineSegment(mousePosition.ToXZ(), shapeCreator.wayPoints[i].ToXZ(), nextPointInShape.ToXZ());
				if (dstFromMouseToLine < closestLineDst)
				{
					closestLineDst = dstFromMouseToLine;
					mouseOverLineIndex = i;
				}
			}

			if (selectionInfo.lineIndex != mouseOverLineIndex)
			{
				selectionInfo.lineIndex = mouseOverLineIndex;
				selectionInfo.mouseIsOverLine = mouseOverLineIndex != -1;
				needsRepaint = true;
			}
		}
	}

	public class SelectionInfo
	{
		public int pointIndex = -1;
		public bool mouseIsOverPoint;
		public bool pointIsSelected;
		public Vector3 positionAtStartOfDrag;

		public int lineIndex = -1;
		public bool mouseIsOverLine;
	}
}
