using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*[CustomPropertyDrawer(typeof(SimpleLayoutGenerator.SpawnOptions))]
public class SpawnOptionsDrawer : PropertyDrawer
{
	SerializedProperty enableSpawning;
	bool folded = false;

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float height = 17f;

		if (folded && enableSpawning.boolValue)
		{
			height *= 11;
		}

		return height;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		enableSpawning = property.FindPropertyRelative("enableSpawning");

		EditorGUI.BeginProperty(position, label, property);
		Rect labelRect = new Rect(position.x, position.y, position.width / 2, 17);
		Rect enableRect = new Rect(position.x + position.width / 2, position.y, 
			position.width / 2, position.height);

		if (enableSpawning.boolValue)
		{
			folded = EditorGUI.Foldout(labelRect, folded, label, true);
			if (folded)
			{
				Rect typeRect = new Rect(position.x, position.y + 17, position.width, 17);
				Rect amountRect = new Rect(position.x, position.y + 17 * 2, position.width, 17*3);
				Rect widthRect = new Rect(position.x, position.y + 17 * 5, position.width, 17*3);
				Rect lengthRect = new Rect(position.x, position.y + 17 * 8, position.width, 17*3);

				EditorGUI.SelectableLabel(typeRect, ((RegionType) property.FindPropertyRelative("type").enumValueIndex).ToString());
				EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("amount"));
				EditorGUI.PropertyField(widthRect, property.FindPropertyRelative("width"));
				EditorGUI.PropertyField(lengthRect, property.FindPropertyRelative("length"));
			}
		}
		else
		{
			EditorGUI.LabelField(labelRect, label);
		}
		
		EditorGUI.PropertyField(enableRect, enableSpawning);
		EditorGUI.EndProperty();
	}
}


[CustomPropertyDrawer(typeof(SimpleLayoutGenerator.Range))]
public class RangeDrawer : PropertyDrawer
{

	private SerializedProperty minValue;
	private SerializedProperty maxValue;

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return 51;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		minValue = property.FindPropertyRelative("min");
		maxValue = property.FindPropertyRelative("max");

		EditorGUI.BeginProperty(position, label, property);

		var indent = EditorGUI.indentLevel;
		//EditorGUI.indentLevel = 0;

		EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
		var minRect = new Rect(position.x, position.y + 17, position.width, 16);
		var maxRect = new Rect(position.x, position.y + 34, position.width, 16);

		EditorGUI.IntSlider(minRect, minValue, 0, maxValue.intValue);
		EditorGUI.IntSlider(maxRect, maxValue, minValue.intValue, 20);

		EditorGUI.EndProperty();
	}
}

[CustomPropertyDrawer(typeof(CustomRegion))]
public class PremadeRegionsDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return 53;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		// Stop child indentation
		var indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		float third = position.width / 3f;

		// Calculate rects
		var nameRect = new Rect(position.x, position.y, third * 2, 16);
		var typeRect = new Rect(position.x + third*2 + 5, position.y, third - 10, 16);
		var boundRect = new Rect(position.x, position.y + 17, position.width, 32);

		// Draw fields - pass GUIContent.nonr to each so no labels are drawn
		//EditorGUI.LabelField(position, label);
		//EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("name"), GUIContent.none);
		EditorGUI.PropertyField(typeRect, property.FindPropertyRelative("type"), GUIContent.none);
		EditorGUI.PropertyField(boundRect, property.FindPropertyRelative("bounds"), GUIContent.none);

		// Set indentation back to normal
		EditorGUI.indentLevel = indent;

		EditorGUI.EndProperty();
	}
}

[CustomEditor(typeof(SimpleLayoutGenerator))]
public class LayoutGeneratorEditor : Editor {

	#region References for Scene Gui
	SimpleLayoutGenerator layoutGenerator;
	SelectionInfo selectionInfo;
	bool needsRepaint;
	#endregion

	#region References for Inspector Gui
	#endregion

	private void OnEnable()
	{
		layoutGenerator = target as SimpleLayoutGenerator;
		selectionInfo = new SelectionInfo();
	}

	#region Inspector Gui Stuff

	#endregion

	#region Scene Gui Stuff

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
		if (!selectionInfo.regionIsSelected)
		{
			UpdateMouseOverInfo(Vector3Int.RoundToInt(mousePosition));
		}
	}

	void HandleLeftMouseDown(Vector3 mousePosition)
	{
		if (!selectionInfo.mouseIsOverRegion)
		{
			Undo.RecordObject(layoutGenerator, "Add Premade Object");
			layoutGenerator.customRegions.Add(new CustomRegion(mousePosition));
			selectionInfo.regionIndex = layoutGenerator.customRegions.Count - 1;
		}
		selectionInfo.regionIsSelected = true;
		selectionInfo.previousObject = layoutGenerator.customRegions[selectionInfo.regionIndex];
		needsRepaint = true;
	}

	void HandleLeftMouseUp(Vector3 mousePosition)
	{
		if (selectionInfo.regionIsSelected)
		{
			layoutGenerator.customRegions[selectionInfo.regionIndex] = selectionInfo.previousObject;
			Undo.RecordObject(layoutGenerator, "Move Premade Object");

			CustomRegion obj = selectionInfo.previousObject;
			obj.bounds.position += Vector3Int.RoundToInt(mousePosition - obj.bounds.center);
			layoutGenerator.customRegions[selectionInfo.regionIndex] = obj;

			selectionInfo.regionIsSelected = false;
			selectionInfo.regionIndex = -1;
			needsRepaint = true;
		}
	}

	void HandleLeftMouseDrag(Vector3 mousePosition)
	{
		if (selectionInfo.regionIsSelected)
		{
			CustomRegion obj = layoutGenerator.customRegions[selectionInfo.regionIndex];
			obj.bounds.position += Vector3Int.RoundToInt(mousePosition - obj.bounds.center);
			layoutGenerator.customRegions[selectionInfo.regionIndex] = obj;

			needsRepaint = true;
		}
	}

	void UpdateMouseOverInfo(Vector3Int mousePosition)
	{
		int mouseOverPointIndex = -1;
		for (int i = 0; i < layoutGenerator.customRegions.Count; i++)
		{
			if (layoutGenerator.customRegions[i].bounds.Contains(mousePosition))
			{
				mouseOverPointIndex = i;
				break;
			}
		}

		if (mouseOverPointIndex != selectionInfo.regionIndex)
		{
			selectionInfo.regionIndex = mouseOverPointIndex;
			selectionInfo.mouseIsOverRegion = mouseOverPointIndex != -1;

			needsRepaint = true;
		}
	}

	void Draw()
	{
		for (int i = 0; i < layoutGenerator.customRegions.Count; i++)
		{
			CustomRegion obj = layoutGenerator.customRegions[i];
			Rect rect = new Rect(new Vector2(obj.bounds.xMin, obj.bounds.yMin), 
				new Vector2(obj.bounds.size.x, obj.bounds.size.y));
			Color fill = Color.green;
			Color outline = Color.magenta;
			if (i == selectionInfo.regionIndex)
			{
				fill = (selectionInfo.regionIsSelected)? Color.red : Color.blue;
				outline = Color.green;
			}
			Handles.DrawSolidRectangleWithOutline(rect, fill, outline);
		}
		needsRepaint = false;
	}

	public class SelectionInfo
	{
		public int regionIndex = -1;
		public bool mouseIsOverRegion;
		public bool regionIsSelected;
		public CustomRegion previousObject;

		public override string ToString()
		{
			return ("Object Index: " + this.regionIndex + "\n"
				+ "Is Mouse over Object? " + this.mouseIsOverRegion + "\n"
				+ "Is Object selected? " + this.regionIsSelected);
		}
	}
	#endregion
}*/
