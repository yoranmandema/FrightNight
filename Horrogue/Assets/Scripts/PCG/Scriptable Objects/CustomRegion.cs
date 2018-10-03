using System;
using System.Collections.Generic;
using UnityEngine;

// Container for regions that are placed beforehand
[Serializable]
public struct CustomRegion
{

	public RegionType type;
	public BoundsInt bounds;
	public bool isSpawn;

	public CustomRegion(Vector3 center)
	{
		this.type = RegionType.None;
		Vector3Int bottomLeft = Vector3Int.RoundToInt(center) - new Vector3Int(5, 5, 0);
		this.bounds = new BoundsInt(bottomLeft, new Vector3Int(10, 10, 1));
		this.isSpawn = true;
	}
	public CustomRegion (Vector3Int pos, Vector3Int size, RegionType type, bool isSpawn)
	{
		this.type = type;
		this.bounds = new BoundsInt(pos, size);
		this.isSpawn = isSpawn;
	}
}