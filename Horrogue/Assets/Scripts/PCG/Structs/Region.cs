using System;
using System.Collections.Generic;
using UnityEngine;

// Types of regions that can be generated
public enum RegionType
{
	None,
	MainCorridor,
	Corridor,
	SpecialRoom,
	SmallRoom,
	MediumRoom,
	LargeRoom,
	Storage,
	Toilet
}

[Serializable]
public class Region
{
    public enum Direction
    {
        NORTH,
        EAST,
        SOUTH,
        WEST
    }

    [Serializable]
	public struct Wall
	{
		public BoundsInt bounds;
		public Direction dir;

		public Wall(Direction dir, BoundsInt bounds)
		{
			this.dir = dir;
			this.bounds = bounds;
		}
	}

	public bool isSpawn = false;
	public bool isConnected;
	public List<Region> connectedRooms;
	public List<Wall> walls;
	public BoundsInt bounds;
	public RegionType type;
    public Direction orientation;

	public Region(BoundsInt bounds, RegionType type)
	{
		this.isConnected = false;
		this.connectedRooms = new List<Region>();
		this.walls = new List<Wall>();
		this.bounds = bounds;
		this.type = type;
		GenerateWallsFromBounds();
	}

	private void GenerateWallsFromBounds()
	{
		int corner = 1;

		Vector3Int pos = bounds.position;
		Vector3Int size = bounds.size;

		BoundsInt nBounds = new BoundsInt(pos.x + corner, pos.y + size.y - 1, pos.z, size.x - corner * 2, 1, size.z);
		if (nBounds.size.x > 0)
		{
			Wall nWall = new Wall(Direction.NORTH, nBounds);
			this.walls.Add(nWall);
		}

		BoundsInt eBounds = new BoundsInt(pos.x + size.x - 1, pos.y + corner, pos.z, 1, size.y - corner * 2, size.z);
		if (eBounds.size.y > 0)
		{
			Wall eWall = new Wall(Direction.EAST, eBounds);
			this.walls.Add(eWall);
		}

		BoundsInt sBounds = new BoundsInt(pos.x + corner, pos.y, pos.z, size.x - corner * 2, 1, size.z);
		if (sBounds.size.x > 0)
		{
			Wall sWall = new Wall(Direction.SOUTH, sBounds);
			this.walls.Add(sWall);
		}

		BoundsInt wBounds = new BoundsInt(pos.x, pos.y + corner, pos.z, 1, size.y - corner * 2, size.z);
		if (wBounds.size.y > 0)
		{
			Wall wWall = new Wall(Direction.WEST, wBounds);
			this.walls.Add(wWall);
		}
	}
    public List<Wall> GetPerpendicularWalls()
    {
        if (orientation == Direction.NORTH || orientation == Direction.SOUTH)
        {
            return walls.FindAll(x => x.dir == Direction.EAST || x.dir == Direction.WEST);
        }
        return walls.FindAll(x => x.dir == Direction.NORTH || x.dir == Direction.SOUTH);
    }
}