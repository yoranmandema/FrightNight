using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class VariantRegionConvertor : BaseRegionConvertor {

	[Header("Variant Region Parameters")]
	public GameObject variableFurnitureParent;

	[Header("Variant Region Output")]
	public VariantRegion variantRegionObject;
	public bool createPrefab = false;

	protected List<VariableRegionFurnitures> variableFurnitures;

	private void OnValidate()
	{
		Convert();
	}

	protected override void Convert()
	{
		base.Convert();

		// Convert variable region furniture
		variableFurnitures = new List<VariableRegionFurnitures>();
		for (int i = 0; i < variableFurnitureParent.transform.childCount; i++)
		{
			GameObject obj = variableFurnitureParent.transform.GetChild(i).gameObject;
			if (obj.transform.childCount > 0)
			{
				VariableRegionFurnitures vrf = new VariableRegionFurnitures(obj.name);

				for (int j = 0; j < obj.transform.childCount; j++)
				{
					vrf.AddPosition(obj.transform.GetChild(j).transform.position);
				}

				variableFurnitures.Add(vrf);
			}
		}

		GenerateVariantRegion();
	}

	private void GenerateVariantRegion()
	{
		variantRegionObject = (VariantRegion)ScriptableObject.CreateInstance(typeof(VariantRegion));
		variantRegionObject.Init(this.name, innerRegionWidth, innerRegionLength, furnitures, connections, tileset, variableFurnitures);

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
			if (variableFurnitures != null)
			{
				Gizmos.color = Color.cyan;
				for (int i = 0; i < variableFurnitures.Count; i++)
				{
					VariableRegionFurnitures vrf = variableFurnitures[i];
					for (int j = 0; j < vrf.spawnLocations.Count; j++)
					{
						// Convert to world coords
						Gizmos.DrawCube(vrf.spawnLocations[j], Vector3.one * 0.4f);
					}
				}
			}
		}
	}
}
