using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RegionConnections {
	public Direction direction;
	public List<BoundsInt> boundsList;

	public RegionConnections(string direction)
	{
		this.boundsList = new List<BoundsInt>();
		this.direction = (direction.ToUpper() == Direction.NORTH.ToString()) ? Direction.NORTH
			: (direction.ToUpper() == Direction.EAST.ToString()) ? Direction.EAST
			: (direction.ToUpper() == Direction.SOUTH.ToString()) ? Direction.SOUTH
			: (direction.ToUpper() == Direction.WEST.ToString()) ? Direction.WEST
			: Direction.UNKOWN;
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
