using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class PremadeRegionConvertor : BaseRegionConvertor {


	[Header("Premade Region Output")]
	public PremadeRegion premadeRegionObject;
	public bool createPrefab = false;

	protected override void ApplyRegionToScriptableObject()
	{
		if (createPrefab)
		{
			premadeRegionObject = (PremadeRegion)ScriptableObject.CreateInstance(typeof(PremadeRegion));
		}

		if (premadeRegionObject != null && (createPrefab || updatePrefab))
		{
			premadeRegionObject.Init(this.name, innerRegionWidth, innerRegionLength, regionFurnitures, regionConnections, floorTileset);

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
			string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + gameObject.name + " (Premade Region).asset");
			AssetDatabase.CreateAsset(premadeRegionObject, assetPathAndName);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = premadeRegionObject;

			createPrefab = false;
		}
	}
}
