using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SimpleLayoutGenerator))]
public class LayoutGeneratorEditor : Editor {

	SimpleLayoutGenerator layoutGenerator;
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
			Undo.RecordObject(layoutGenerator, "Add Premade Object");
			layoutGenerator.premadeLayoutObjects.Add(new SimpleLayoutGenerator.PremadeLayoutObject(mousePosition));
			needsRepaint = true;
		}
	}

	void Draw()
	{
		for (int i = 0; i < layoutGenerator.premadeLayoutObjects.Count; i++)
		{
			SimpleLayoutGenerator.PremadeLayoutObject obj = layoutGenerator.premadeLayoutObjects[i];
			Rect rect = new Rect(new Vector2(obj.regionBounds.xMin, obj.regionBounds.yMin), new Vector2(obj.regionBounds.size.x, obj.regionBounds.size.y));
			Handles.DrawSolidRectangleWithOutline(rect, Color.green, Color.magenta);
		}
		needsRepaint = false;
	}

	private void OnEnable()
	{
		layoutGenerator = target as SimpleLayoutGenerator;
	}
}
