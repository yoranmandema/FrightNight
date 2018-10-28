using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct RegionConnections {
	public Region.Direction direction;
	public List<BoundsInt> boundsList;

	public RegionConnections(string direction)
	{
		this.boundsList = new List<BoundsInt>();
		this.direction = (direction.ToUpper() == Region.Direction.NORTH.ToString()) ? Region.Direction.NORTH
			: (direction.ToUpper() == Region.Direction.EAST.ToString()) ? Region.Direction.EAST
			: (direction.ToUpper() == Region.Direction.SOUTH.ToString()) ? Region.Direction.SOUTH
			: (direction.ToUpper() == Region.Direction.WEST.ToString()) ? Region.Direction.WEST
			: Region.Direction.UNKOWN;
	}

	public void AddConnection(Bounds bounds)
	{
		AddConnection(new BoundsInt(Vector3Int.RoundToInt(bounds.min), Vector3Int.RoundToInt(bounds.size)));
	}

	public void AddConnection(BoundsInt bounds)
	{
		boundsList.Add(bounds);
	}
}
