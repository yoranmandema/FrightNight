using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class PremadeRegionConvertor : MonoBehaviour {

	[Tooltip("Refresh output (Useful after modifiying children)")]
	public bool update = false;

	public bool showGizmos = false;

	[Header("Premade Region Parameters")]
	public int innerRegionWidth;
	public int innerRegionLength;
	public Tileset tileset;
	public GameObject furnitureParent;
	public GameObject connectionParent;

	[Header("Region Output")]
	public PremadeRegion premadeRegionObject;
	public bool createPrefab = false;

	private Vector3 pos;
	private Vector3 size;

	private List<RegionFurnitures> furnitures;
	private List<RegionConnections> connections;

	private void OnValidate()
	{
		update = false;

		size = new Vector3(innerRegionWidth, innerRegionLength);
		pos = transform.position;

		furnitures = new List<RegionFurnitures>();
		for (int i = 0; i < furnitureParent.transform.childCount; i++)
		{
			GameObject obj = furnitureParent.transform.GetChild(i).gameObject;
			if (obj.transform.childCount > 0)
			{
				RegionFurnitures rf = new RegionFurnitures(obj.name);

				for(int j = 0; j < obj.transform.childCount; j++)
				{
					rf.AddPosition(obj.transform.GetChild(j).transform.position);
				}

				furnitures.Add(rf);
			}
		}

		connections = new List<RegionConnections>();
		for (int i = 0; i < connectionParent.transform.childCount; i++)
		{
			GameObject obj = connectionParent.transform.GetChild(i).gameObject;
			Component[] cmps = obj.GetComponents(typeof(BoxCollider2D));

			//Debug.Log(cmps.Length + " colliders found for " + obj.name);

			if (cmps.Length > 0) {
				RegionConnections con = new RegionConnections(obj.name);
				for (int j = 0; j < cmps.Length; j++)
				{
					BoxCollider2D col = (BoxCollider2D)cmps[j];
					con.AddConnection(col.bounds);
				}

				connections.Add(con);
			}
		}

		GeneratePremadeRegion();
	}

	public void GeneratePremadeRegion()
	{
		premadeRegionObject = (PremadeRegion) ScriptableObject.CreateInstance(typeof(PremadeRegion));
		premadeRegionObject.Init(this.name, innerRegionWidth, innerRegionLength, furnitures, connections, tileset);

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

	private void OnDrawGizmos()
	{
		if (showGizmos)
		{
			Gizmos.color = Color.white;
			Gizmos.DrawCube(pos, size + Vector3.one * 2);

			Gizmos.color = Color.green;
			Gizmos.DrawCube(pos + Vector3.forward, size);

			if (furnitures != null)
			{
				Gizmos.color = Color.blue;
				for (int i = 0; i < furnitures.Count; i++)
				{
					RegionFurnitures rf = furnitures[i];
					for (int j = 0; j < rf.positions.Count; j++)
					{
						// Convert to world coords
						Gizmos.DrawCube(rf.positions[j], Vector3.one * 0.25f);
					}
				}
			}

			if (connections != null)
			{
				Gizmos.color = Color.magenta;
				for (int i = 0; i < connections.Count; i++)
				{
					RegionConnections rc = connections[i];
					for (int j = 0; j < rc.boundsList.Count; j++)
					{
						// Are world coords already
						Gizmos.DrawCube(rc.boundsList[j].center, rc.boundsList[j].size - Vector3.one * 0.25f);
					}
				}
			}
		}
	}
}
