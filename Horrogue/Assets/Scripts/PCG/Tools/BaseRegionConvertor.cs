using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class BaseRegionConvertor : MonoBehaviour {

	[Tooltip("Refresh output (Useful after modifiying children)")]
	public bool update = false;
	public bool updatePrefab = false;

	public bool drawGizmos = false;

	[Header("Region Parameters")]
	public int innerRegionWidth;
	public int innerRegionLength;

	public Tileset floorTileset;
	public Tileset wallTileset;

	[Header("Rendering")]
	public Material spriteMaterial;

	[Header("Region Furnitures")]
	public List<RegionFurnitures> regionFurnitures;

	[Header("Region Connections")]
	public List<RegionConnections> regionConnections;

	protected Vector3 pos;
	protected Vector3 size;

	protected GameObject[,] tileMap;
	bool refreshTiles = false;

	protected GameObject tileParent;
	protected GameObject furnitureParent;
	protected GameObject connectionParent;

	protected const string FURNITURE_PARENT_NAME = "Furniture Parent",
		TILE_PARENT_NAME = "Tile Parent",
		CONNECTION_PARENT_NAME = "Connection Parent";

	protected Dictionary<GameObject, RegionFurnitures> furnitureObjectsParents;
	protected Dictionary<GameObject, RegionConnections> connectionObjectParents;
	/*protected Dictionary<GameObject, List<GameObject>> furnitureObjects;
	protected Dictionary<GameObject, List<GameObject>> connectionObjects;*/

	protected virtual void OnValidate()
	{
		CheckParents();

		if (update)
		{
			//ApplyInspectorValues();
			Convert();
		}

		ApplyRegionToScriptableObject();

		update = false;

		if (spriteMaterial != null)
		{
			SpriteRenderer[] sprs = transform.GetComponentsInChildren<SpriteRenderer>();
			for (int i = 0; i < sprs.Length; i++)
			{
				StartCoroutine(ApplySpriteMaterial(sprs[i]));
			}
		}
		
	}

	private IEnumerator ApplySpriteMaterial(SpriteRenderer sr)
	{
		sr.material = spriteMaterial;
		yield return new WaitForEndOfFrame();
	}

	private void RenderTiles()
	{
		if (floorTileset != null && wallTileset != null)
		{
			tileMap = new GameObject[innerRegionWidth + 2, innerRegionLength + 2];
			for (int x = 0; x < innerRegionWidth + 2; x++)
			{
				for (int y = 0; y < innerRegionLength + 2; y++)
				{
					GameObject tilePrefab;
					if (wallTileset != null && (y == 0 || y == innerRegionLength + 1 || x == 0 || x == innerRegionWidth + 1))
					{
						tilePrefab = wallTileset.Middle;
					} 
					else
					{
						tilePrefab = floorTileset.Middle;
					}

					Vector3 tilePos = new Vector3(pos.x - size.x / 2f + (x + 0.5f) - 1, pos.y - size.y / 2f + (y + 0.5f) - 1);

					tileMap[x, y] = Instantiate(tilePrefab, tilePos, Quaternion.identity, tileParent.transform);
					if (spriteMaterial != null) tileMap[x, y].GetComponent<SpriteRenderer>().material = spriteMaterial;
				}
			}
		}

		refreshTiles = false;
	}

	IEnumerator Destroy(GameObject go)
	{
		yield return new WaitForEndOfFrame();
		DestroyImmediate(go);
		go = null;
		refreshTiles = true;
	}

	protected virtual void ApplyInspectorValues()
	{
		/*if (regionFurnitures == null) regionFurnitures = new List<RegionFurnitures>();
		if (furnitureObjectsParents == null) furnitureObjectsParents = new Dictionary<RegionFurnitures, GameObject>();
		for (int i = 0; i < regionFurnitures.Count; i++)
		{
			RegionFurnitures rf = regionFurnitures[i];
			GameObject fObjRef;
			if (!furnitureObjectsParents.ContainsKey(rf))
			{
				rf.name = "New Region Furnitures";
				fObjRef = new GameObject();
				fObjRef.transform.SetParent(furnitureParent.transform);
				furnitureObjectsParents.Add(rf, fObjRef);


			}
			else
			{
				fObjRef = furnitureObjectsParents[rf];
			}

			fObjRef.name = rf.name;

			if (rf.prefab != null)
			{
				List<GameObject> childObjRefs = furnitureObjects[fObjRef];

				for (int j = 0; j < rf.spawnTransforms.Count; j++)
				{
					
				}
			}

		}*/
	}

	protected virtual void CheckParents()
	{
		if (innerRegionWidth != size.x || innerRegionLength != size.y || transform.position != pos)
		{
			size = new Vector3(innerRegionWidth, innerRegionLength);
			pos = transform.position;

			StartCoroutine(Destroy(tileParent));
		}
		
		if (furnitureObjectsParents == null)
		{
			furnitureObjectsParents = new Dictionary<GameObject, RegionFurnitures>();
		}
		if (connectionObjectParents == null)
		{
			connectionObjectParents = new Dictionary<GameObject, RegionConnections>();
		}

		Transform t = transform.Find(FURNITURE_PARENT_NAME);
		furnitureParent = ( t != null) ? t.gameObject : null;
		if (furnitureParent == null)
		{
			furnitureParent = new GameObject(FURNITURE_PARENT_NAME);
			furnitureParent.transform.SetParent(transform);
			furnitureParent.transform.localPosition = Vector3.zero;
		}

		t = transform.Find(CONNECTION_PARENT_NAME);
		connectionParent = (t != null) ? t.gameObject : null;
		if (connectionParent == null)
		{
			connectionParent = new GameObject(CONNECTION_PARENT_NAME);
			connectionParent.transform.SetParent(transform);
			connectionParent.transform.localPosition = Vector3.zero;
		}

		t = transform.Find(TILE_PARENT_NAME);
		tileParent = (t != null) ? t.gameObject : null;
		if (refreshTiles || (tileParent == null && floorTileset != null && wallTileset != null))
		{
			tileParent = new GameObject(TILE_PARENT_NAME);
			tileParent.transform.SetParent(transform);
			tileParent.transform.localPosition = Vector3.zero;

			RenderTiles();
		}
	}

	protected virtual void Convert()
	{
		//if (regionFurnitures == null)
		regionFurnitures = new List<RegionFurnitures>();
		for (int i = 0; i < furnitureParent.transform.childCount; i++)
		{
			GameObject obj = furnitureParent.transform.GetChild(i).gameObject;
			if (obj.transform.childCount > 0)
			{
				RegionFurnitures rf = new RegionFurnitures(obj.name);

				for (int j = 0; j < obj.transform.childCount; j++)
				{
					Transform trans = obj.transform.GetChild(j).transform;
					rf.AddPosition(trans.localPosition, trans.rotation, trans.localScale);
				}

				regionFurnitures.Add(rf);
			}
		}

		//if (regionConnections == null)
		regionConnections = new List<RegionConnections>();
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
					Bounds bounds = ((BoxCollider2D)cmps[j]).bounds;
					bounds.min -= pos;
					bounds.max -= pos;
					con.AddConnection(bounds);
				}

				regionConnections.Add(con);
			}
		}
	}

	protected abstract void ApplyRegionToScriptableObject();

	protected virtual void OnDrawGizmos()
	{
		if (drawGizmos)
		{
			Gizmos.color = Color.white;
			Gizmos.DrawWireCube(pos, size + Vector3.one * 2.1f);

			Gizmos.color = Color.green;
			Gizmos.DrawWireCube(pos + Vector3.forward, size - Vector3.one * 0.1f);

			if (regionFurnitures != null)
			{
				Gizmos.color = Color.blue;
				for (int i = 0; i < regionFurnitures.Count; i++)
				{
					RegionFurnitures rf = regionFurnitures[i];
					for (int j = 0; j < rf.spawnTransforms.Count; j++)
					{
						// Convert to world coords
						Gizmos.DrawCube(rf.spawnTransforms[j].position + pos, Vector3.one * 0.25f);
					}
				}
			}

			if (regionConnections != null)
			{
				Gizmos.color = Color.magenta;
				for (int i = 0; i < regionConnections.Count; i++)
				{
					RegionConnections rc = regionConnections[i];
					for (int j = 0; j < rc.boundsList.Count; j++)
					{
						// Are world coords already
						Gizmos.DrawCube(rc.boundsList[j].center + pos, rc.boundsList[j].size - Vector3.one * 0.25f);
					}
				}
			}
		}
	}
}
