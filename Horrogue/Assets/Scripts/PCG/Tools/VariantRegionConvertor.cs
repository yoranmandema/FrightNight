using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class VariantRegionConvertor : BaseRegionConvertor {

	[Header("Variable Region Furnitures")]
	public List<VariableRegionFurnitures> variableRegionFurnitures;

	[Header("Variant Region Output")]
	public VariantRegion variantRegionObject;
	public bool createPrefab = false;

	protected GameObject variableFurnitureParent;

	private const string VARIABLE_FURNITURE_PARENT_NAME = "Variable Furniture Parent";

	protected override void CheckParents()
	{
		Transform t = transform.Find(VARIABLE_FURNITURE_PARENT_NAME);
		variableFurnitureParent = (t != null) ? t.gameObject : null;
		if (variableFurnitureParent == null)
		{
			variableFurnitureParent = new GameObject(VARIABLE_FURNITURE_PARENT_NAME);
			variableFurnitureParent.transform.SetParent(transform);
		}
		base.CheckParents();
	}

	protected override void Convert()
	{
		base.Convert();

		// Convert variable region furniture
		variableRegionFurnitures = new List<VariableRegionFurnitures>();
		for (int i = 0; i < variableFurnitureParent.transform.childCount; i++)
		{
			GameObject obj = variableFurnitureParent.transform.GetChild(i).gameObject;
			if (obj.transform.childCount > 0)
			{
				VariableRegionFurnitures vrf = new VariableRegionFurnitures(obj.name);

				for (int j = 0; j < obj.transform.childCount; j++)
				{
					Transform trans = obj.transform.GetChild(j).transform;
					vrf.AddPosition(trans.localPosition, trans.rotation, trans.localScale);
				}

				variableRegionFurnitures.Add(vrf);
			}
		}
	}

	protected override void ApplyRegionToScriptableObject()
	{
		if (createPrefab)
		{
			variantRegionObject = (VariantRegion)ScriptableObject.CreateInstance(typeof(VariantRegion));
		}
		if (variantRegionObject != null && (createPrefab || updatePrefab))
		{
			variantRegionObject.Init(this.name, innerRegionWidth, innerRegionLength, regionFurnitures, regionConnections, floorTileset, variableRegionFurnitures);

			updatePrefab = false;
		}

		if (createPrefab)
		{
			// http://wiki.unity3d.com/index.php/CreateScriptableObjectAsset

			string path = AssetDatabase.GetAssetPath(Selection.activeObject);
			if (path == "")
			{
				path = "Assets";
			}
			else if (Path.GetExtension(path) != "")
			{
				path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
			}

			// Create Premade Region
			string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + gameObject.name + " (Variant Region).asset");
			AssetDatabase.CreateAsset(variantRegionObject, assetPathAndName);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = variantRegionObject;

			createPrefab = false;
		}
	}

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();

		if (drawGizmos)
		{
			if (variableRegionFurnitures != null)
			{
				Gizmos.color = Color.cyan;
				for (int i = 0; i < variableRegionFurnitures.Count; i++)
				{
					VariableRegionFurnitures vrf = variableRegionFurnitures[i];
					for (int j = 0; j < vrf.spawnTransforms.Count; j++)
					{
						// Convert to world coords
						Gizmos.DrawCube(vrf.spawnTransforms[j].position + pos, Vector3.one * 0.4f);
					}
				}
			}
		}
	}
}
