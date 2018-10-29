using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseRegionConvertor : MonoBehaviour {

	[Tooltip("Refresh output (Useful after modifiying children)")]
	public bool update = false;

	public bool drawGizmos = false;

	[Header("Region Parameters")]
	public int innerRegionWidth;
	public int innerRegionLength;
	public Tileset tileset;
	public GameObject furnitureParent;
	public GameObject connectionParent;

	protected Vector3 pos;
	protected Vector3 size;

	protected List<RegionFurnitures> furnitures;
	protected List<RegionConnections> connections;

	protected virtual void Convert()
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

				for (int j = 0; j < obj.transform.childCount; j++)
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

			if (cmps.Length > 0)
			{
				RegionConnections con = new RegionConnections(obj.name);
				for (int j = 0; j < cmps.Length; j++)
				{
					BoxCollider2D col = (BoxCollider2D)cmps[j];
					con.AddConnection(col.bounds);
				}

				connections.Add(con);
			}
		}
	}

	protected virtual void OnDrawGizmos()
	{
		if (drawGizmos)
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
					for (int j = 0; j < rf.spawnLocations.Count; j++)
					{
						// Convert to world coords
						Gizmos.DrawCube(rf.spawnLocations[j], Vector3.one * 0.25f);
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
