using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SimpleLayoutGenerator))]
public class LayoutGeneratorEditor : Editor {

	SimpleLayoutGenerator layoutGenerator;
	SelectionInfo selectionInfo;
	bool needsRepaint;

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
		Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
		float drawPlaneHeight = 0;
		float dstToDrawPlane = (drawPlaneHeight - mouseRay.origin.z) / mouseRay.direction.z;
		Vector3 mousePosition = mouseRay.GetPoint(dstToDrawPlane);

		if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
		{
			HandleLeftMouseDown(mousePosition);
		}

		if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
		{
			HandleLeftMouseUp(mousePosition);
		}

		if (guiEvent.type == EventType.MouseDrag && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
		{
			HandleLeftMouseDrag(mousePosition);
		}
		if (!selectionInfo.objectIsSelected)
		{
			UpdateMouseOverInfo(Vector3Int.RoundToInt(mousePosition));
		}
	}

	void HandleLeftMouseDown(Vector3 mousePosition)
	{
		if (!selectionInfo.mouseIsOverObject)
		{
			Undo.RecordObject(layoutGenerator, "Add Premade Object");
			layoutGenerator.premadeObjects.Add(new PremadeLayoutObject(mousePosition));
			selectionInfo.objectIndex = layoutGenerator.premadeObjects.Count - 1;
		}
		selectionInfo.objectIsSelected = true;
		selectionInfo.previousObject = layoutGenerator.premadeObjects[selectionInfo.objectIndex];
		needsRepaint = true;
	}

	void HandleLeftMouseUp(Vector3 mousePosition)
	{
		if (selectionInfo.objectIsSelected)
		{
			Debug.Log(selectionInfo.previousObject.ToString());
			layoutGenerator.premadeObjects[selectionInfo.objectIndex] = selectionInfo.previousObject;
			Undo.RecordObject(layoutGenerator, "Move Premade Object");

			PremadeLayoutObject obj = selectionInfo.previousObject;
			obj.regionBounds.position += Vector3Int.RoundToInt(mousePosition - obj.regionBounds.center);
			layoutGenerator.premadeObjects[selectionInfo.objectIndex] = obj;

			selectionInfo.objectIsSelected = false;
			selectionInfo.objectIndex = -1;
			needsRepaint = true;
		}
	}

	void HandleLeftMouseDrag(Vector3 mousePosition)
	{
		if (selectionInfo.objectIsSelected)
		{
			PremadeLayoutObject obj = layoutGenerator.premadeObjects[selectionInfo.objectIndex];
			obj.regionBounds.position += Vector3Int.RoundToInt(mousePosition - obj.regionBounds.center);
			layoutGenerator.premadeObjects[selectionInfo.objectIndex] = obj;

			needsRepaint = true;
		}
	}

	void UpdateMouseOverInfo(Vector3Int mousePosition)
	{
		int mouseOverPointIndex = -1;
		for (int i = 0; i < layoutGenerator.premadeObjects.Count; i++)
		{
			if (layoutGenerator.premadeObjects[i].regionBounds.Contains(mousePosition))
			{
				mouseOverPointIndex = i;
				break;
			}
		}

		if (mouseOverPointIndex != selectionInfo.objectIndex)
		{
			selectionInfo.objectIndex = mouseOverPointIndex;
			selectionInfo.mouseIsOverObject = mouseOverPointIndex != -1;

			needsRepaint = true;
		}
	}


	void Draw()
	{
		for (int i = 0; i < layoutGenerator.premadeObjects.Count; i++)
		{
			PremadeLayoutObject obj = layoutGenerator.premadeObjects[i];
			Rect rect = new Rect(new Vector2(obj.regionBounds.xMin, obj.regionBounds.yMin), 
				new Vector2(obj.regionBounds.size.x, obj.regionBounds.size.y));
			Color fill = Color.green;
			Color outline = Color.magenta;
			if (i == selectionInfo.objectIndex)
			{
				fill = (selectionInfo.objectIsSelected)? Color.red : Color.blue;
				outline = Color.green;
			}
			Handles.DrawSolidRectangleWithOutline(rect, fill, outline);
		}
		needsRepaint = false;
	}

	private void OnEnable()
	{
		layoutGenerator = target as SimpleLayoutGenerator;
		selectionInfo = new SelectionInfo();
	}

	public class SelectionInfo
	{
		public int objectIndex = -1;
		public bool mouseIsOverObject;
		public bool objectIsSelected;
		public PremadeLayoutObject previousObject;

		public override string ToString()
		{
			return ("Object Index: " + this.objectIndex + "\n"
				+ "Is Mouse over Object? " + this.mouseIsOverObject + "\n"
				+ "Is Object selected? " + this.objectIsSelected);
		}
	}

}
