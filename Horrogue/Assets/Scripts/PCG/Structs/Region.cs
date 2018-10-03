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
	[Serializable]
	public struct Wall
	{
		public enum Direction
		{
			NORTH,
			EAST,
			SOUTH,
			WEST
		}

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

	public Region(BoundsInt bounds, RegionType type)
	{
		this.isConnected = false;
		this.connectedRooms = new List<Region>();
		this.walls = new List<Wall>();
		this.bounds = bounds;
		this.type = type;
		GenerateWallsFromBounds();
	}

	public Wall GetWall(Vector2Int[] direction)
	{
		return new Wall();
	}

	private void GenerateWallsFromBounds()
	{
		int corner = 2;

		Vector3Int pos = bounds.position;
		Vector3Int size = bounds.size;

		BoundsInt nBounds = new BoundsInt(pos.x + corner, pos.y + size.y - 1, pos.z, size.x - corner * 2, 1, size.z);
		if (nBounds.size.x > 0)
		{
			Wall nWall = new Wall(Wall.Direction.NORTH, nBounds);
			this.walls.Add(nWall);
		}

		BoundsInt eBounds = new BoundsInt(pos.x + size.x - 1, pos.y + corner, pos.z, 1, size.y - corner * 2, size.z);
		if (eBounds.size.y > 0)
		{
			Wall eWall = new Wall(Wall.Direction.EAST, eBounds);
			this.walls.Add(eWall);
		}

		BoundsInt sBounds = new BoundsInt(pos.x + corner, pos.y, pos.z, size.x - corner * 2, 1, size.z);
		if (sBounds.size.x > 0)
		{
			Wall sWall = new Wall(Wall.Direction.SOUTH, sBounds);
			this.walls.Add(sWall);
		}

		BoundsInt wBounds = new BoundsInt(pos.x, pos.y + corner, pos.z, 1, size.y - corner * 2, size.z);
		if (wBounds.size.y > 0)
		{
			Wall wWall = new Wall(Wall.Direction.WEST, wBounds);
			this.walls.Add(wWall);
		}
	}

	public Wall[] GetClosestWall(Region otherRegion)
	{
		bool isAbove = true, isRight = true;
		List<Wall> thisWalls = new List<Wall>(), otherWalls = new List<Wall>();
		if (otherRegion.bounds.center.y < bounds.center.y)
		{
			isAbove = false;
			thisWalls.AddRange(walls.FindAll(x => x.dir == Wall.Direction.SOUTH));
			otherWalls.AddRange(otherRegion.walls.FindAll(x => x.dir == Wall.Direction.NORTH));
		}
		else
		{
			//isAbove = true;
			thisWalls.AddRange(walls.FindAll(x => x.dir == Wall.Direction.NORTH));
			otherWalls.AddRange(otherRegion.walls.FindAll(x => x.dir == Wall.Direction.SOUTH));
		}
		if (otherRegion.bounds.center.x < bounds.center.x)
		{
			isRight = false;
			thisWalls.AddRange(walls.FindAll(x => x.dir == Wall.Direction.WEST));
			otherWalls.AddRange(otherRegion.walls.FindAll(x => x.dir == Wall.Direction.EAST));
		}
		else
		{
			//isRight = true;
			thisWalls.AddRange(walls.FindAll(x => x.dir == Wall.Direction.EAST));
			otherWalls.AddRange(otherRegion.walls.FindAll(x => x.dir == Wall.Direction.WEST));
		}
		Wall thisClosestWall = thisWalls[0], otherClosestWall = otherWalls[0];
		for(int i = 1; i < thisWalls.Count; i++)
		{
			for (int j = 0; j < otherWalls.Count; i++)
			{
				if (isAbove && isRight)
				{

				}
				else if (isAbove && !isRight)
				{

				}
				else if (!isAbove && isRight)
				{

				}
				else if (!isAbove && !isRight)
				{

				}
			}


		}

		return new Wall[] { thisClosestWall, otherClosestWall };
	}
}