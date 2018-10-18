using System;
using System.Collections.Generic;
using UnityEngine;

// Types of regions that can be generated
public enum RegionType
{
	None,
	MainCorridor,
	Corridor,
	Toilet,
	ClassRoom,
	Storage,
}

[Serializable]
public struct RegionSpot
{
	public RegionType type;
	public int regID;
	//public Region region;

	public RegionSpot(RegionType type, int id)
	{
		this.type = type;
		this.regID = id;
		//this.region = region;
	}
}

[Serializable]
public class Region
{
    public enum Direction
    {
        NORTH,
        EAST,
        SOUTH,
        WEST,
		UNKOWN
	}

    [Serializable]
	public struct Wall
	{
		public BoundsInt bounds;
		public Direction dir;
		public bool isVertical;

		public Wall(Direction dir, BoundsInt bounds)
		{
			this.dir = dir;
			this.bounds = bounds;
			isVertical = (dir == Direction.EAST || dir == Direction.WEST);
		}

		public bool OverlapsWall(Wall otherWall)
		{
			Bounds thisBounds = new Bounds(bounds.center, bounds.size);
			Bounds otherBounds = new Bounds(otherWall.bounds.center, otherWall.bounds.size);
			//Debug.Log("This [" + dir.ToString() + " -> " + thisBounds.ToString() + "]\nOther [" + otherWall.dir.ToString() + " -> " + otherBounds.ToString() + "]");
			//Debug.Log(thisBounds.Intersects(otherBounds));
			return thisBounds.Intersects(otherBounds);
		}
	}

	public static int NEXT_ID = 0;

	private int id;
	public bool isSpawn = false;
	public bool isConnected;
	public List<Region> connectedRegions;
	public List<BoundsInt> connections;
	public List<Wall> walls;
	public BoundsInt bounds;
	public RegionType type;
    public Direction orientation;
	public const int cornerThreshold = 1; // may change to wall thickness, since it determines corner size
	public const int minWallWidth = 2;

	public int Id
	{
		get
		{
			return id;
		}
	}

	public Region(BoundsInt bounds, RegionType type, Direction orientation = Direction.NORTH)
	{
		id = NEXT_ID++;
		//Debug.Log("New region id = " + Id + " (next = " + NEXT_ID + ")");

		this.isConnected = false;
		this.connectedRegions = new List<Region>();
		this.connections = new List<BoundsInt>();
		this.walls = new List<Wall>();
		this.bounds = bounds;
		this.type = type;
		this.orientation = orientation;
		GenerateWallsFromBounds();
	}

	// Returns wall overlap
	public BoundsInt ConnectToRegion (Region otherRegion, int connectionSize = -1)
	{
		//Debug.Log("Connecting " + this.ToString() + " to " + otherRegion.ToString());
		Dictionary<Wall, Wall> overlappingWalls = GetOverlappingWalls(otherRegion);
		
		// owe -> overlapping walls enumerator
		Dictionary<Wall, Wall>.Enumerator owe = overlappingWalls.GetEnumerator();
		owe.MoveNext();
		// Take entire wall as bounds
		if (overlappingWalls.Count == 1)
		{
			Wall wallA = owe.Current.Key, 
				wallB = owe.Current.Value;
			BoundsInt overlap = CalculateOverlap(wallA.bounds, wallB.bounds);

			if (connectionSize > 0)
			{
				Vector3Int pos, size;
				if (wallA.isVertical) // | Vertical
				{
					pos = Vector3Int.RoundToInt(overlap.center - new Vector3Int(0, connectionSize / 2, 0));
					size = new Vector3Int(1, connectionSize, 1);
				}
				else // __ Horizontal
				{
					pos = Vector3Int.RoundToInt(overlap.center - new Vector3Int(connectionSize / 2, 0, 0));
					size = new Vector3Int(connectionSize, 1, 1);
				}
				overlap = new BoundsInt(pos, size);
			}

			ConnectToRegion(otherRegion, overlap, wallA, wallB);
			return overlap;
		}
		Debug.Log(overlappingWalls.Count);
		throw new Exception("Can't connect Regions! Either none or too many walls are overlapping!"); 
	}
	public void ConnectToRegion (Region otherRegion, BoundsInt connectionBounds, Wall thisWall, Wall otherWall)
	{
		// Add other region to connected regions
		connectedRegions.Add(otherRegion);

		// Check if this region or another region is spawn or connected to spawn
		isConnected = isSpawn || otherRegion.connectedRegions.Find(x => x.isConnected || x.isSpawn) != null;

		// Add this region to the connected regions of the other region
		otherRegion.connectedRegions.Add(this);

		// Split walls and register connection for both regions
		CreateConnection(otherRegion, connectionBounds, thisWall, otherWall);
	}

	private void CreateConnection(Region otherRegion, BoundsInt connectionBounds, Wall thisWall, Wall otherWall)
	{
		// Add connection bounds to both regions
		connections.Add(connectionBounds);
		otherRegion.connections.Add(connectionBounds);

		// Get wall overlap with corners
		Vector2Int dir = GetPerpendicularDirectionVector(thisWall.dir);
		Vector3Int corner = new Vector3Int(cornerThreshold * dir.x, cornerThreshold * dir.y, 0);
		BoundsInt overlap = CalculateOverlap(thisWall.bounds, otherWall.bounds);
		overlap.min -= corner * 2;	// times two to add a new corner
		overlap.max += corner * 2;

		// Split walls
		SplitWall(this, thisWall, overlap);
		SplitWall(otherRegion, otherWall, overlap);
	}

	private void SplitWall(Region region, Wall wall, BoundsInt splitSize)
	{
		Wall wallA = wall, wallB = wall;

		region.walls.Remove(wall);

		Vector3Int dirOffset = new Vector3Int();
		if (!wall.isVertical)
		{
			dirOffset.x = cornerThreshold;
		}
		else
		{
			dirOffset.y = cornerThreshold;
		}

		wallA.bounds.max = splitSize.min + dirOffset;
		wallB.bounds.min = splitSize.max - dirOffset;

		wallA.bounds.size += new Vector3Int(0, 0, 1);
		wallB.bounds.size += new Vector3Int(0, 0, 1);

		if ((wallA.isVertical && wallA.bounds.size.y >= minWallWidth) || (!wallA.isVertical && wallA.bounds.size.x >= minWallWidth))
		{
			region.walls.Add(wallA);
		}
		if ((wallB.isVertical && wallB.bounds.size.y >= minWallWidth) || (!wallB.isVertical && wallB.bounds.size.x >= minWallWidth))
		{
			region.walls.Add(wallB);
		}
	}

	private BoundsInt CalculateOverlap (BoundsInt a, BoundsInt b)
	{
		int minX = (a.xMin > b.xMin) ? a.xMin : b.xMin,
			minY = (a.yMin > b.yMin) ? a.yMin : b.yMin;

		int	maxX = (a.xMax < b.xMax) ? a.xMax : b.xMax,
			maxY = (a.yMax < b.yMax) ? a.yMax : b.yMax;

		return new BoundsInt(minX, minY, 0, maxX - minX, maxY - minY, 1);
	}
	public static bool BoundsOverlap(BoundsInt a, BoundsInt b, int overlapThreshhold = 0)
	{
		/*
				bool topRight = (b.x >= a.x + overlapThreshhold && b.y >= a.y + overlapThreshhold 
					&& b.x <= a.xMax - overlapThreshhold && b.y <= a.yMax - overlapThreshhold);

				bool bottomRight = (b.xMax >= a.x + overlapThreshhold && b.yMax >= a.yMax + overlapThreshhold
					&& b.xMax <= a.xMax - overlapThreshhold && b.yMax <= a.yMax - overlapThreshhold);

				bool bottomLeft = (a.x >= b.x + overlapThreshhold && a.y >= b.y + overlapThreshhold
					&& a.x <= b.xMax - overlapThreshhold && a.y <= b.yMax - overlapThreshhold);

				bool topLeft = (a.xMax >= a.x + overlapThreshhold && a.yMax >= a.yMax + overlapThreshhold
					&& a.xMax <= a.xMax - overlapThreshhold && a.yMax <= a.yMax - overlapThreshhold);

				return (topRight || topLeft || bottomRight || bottomLeft);
				*/
		// Convert IntBounds to Bounds to use Bounds.Intersects()
		Bounds boundsA = new Bounds(a.center, a.size);
		Bounds boundsB = new Bounds(b.center, b.size);

		// Apply threshhold
		boundsA.size -= new Vector3(overlapThreshhold * 2 + 1, overlapThreshhold * 2 + 1);
		boundsB.size -= new Vector3(overlapThreshhold * 2 + 1, overlapThreshhold * 2 + 1);
		boundsA.min += new Vector3(overlapThreshhold + 0.5f, overlapThreshhold + 0.5f);
		boundsB.min += new Vector3(overlapThreshhold + 0.5f, overlapThreshhold + 0.5f);

		//Debug.Log(boundsA.min.ToString() + " and " + boundsB.min.ToString() + " are intersecting? " + boundsA.Intersects(boundsB));

		return (boundsA.Intersects(boundsB) || boundsB.Intersects(boundsA));
	}

	private void GenerateWallsFromBounds()
	{
		Vector3Int pos = bounds.position;
		Vector3Int size = bounds.size;

		BoundsInt nBounds = new BoundsInt(pos.x + cornerThreshold, pos.y + size.y - 1, pos.z, size.x - cornerThreshold * 2, 1, size.z);
		if (nBounds.size.x > 0)
		{
			Wall nWall = new Wall(Direction.NORTH, nBounds);
			this.walls.Add(nWall);
		}

		BoundsInt eBounds = new BoundsInt(pos.x + size.x - 1, pos.y + cornerThreshold, pos.z, 1, size.y - cornerThreshold * 2, size.z);
		if (eBounds.size.y > 0)
		{
			Wall eWall = new Wall(Direction.EAST, eBounds);
			this.walls.Add(eWall);
		}

		BoundsInt sBounds = new BoundsInt(pos.x + cornerThreshold, pos.y, pos.z, size.x - cornerThreshold * 2, 1, size.z);
		if (sBounds.size.x > 0)
		{
			Wall sWall = new Wall(Direction.SOUTH, sBounds);
			this.walls.Add(sWall);
		}

		BoundsInt wBounds = new BoundsInt(pos.x, pos.y + cornerThreshold, pos.z, 1, size.y - cornerThreshold * 2, size.z);
		if (wBounds.size.y > 0)
		{
			Wall wWall = new Wall(Direction.WEST, wBounds);
			this.walls.Add(wWall);
		}
	}

	public static bool ArePerpendicual(Direction a, Direction b)
	{
		if (a == Direction.NORTH || a == Direction.SOUTH)
		{
			if (b == Direction.EAST || b == Direction.WEST)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		else if (b == Direction.NORTH || b == Direction.SOUTH)
		{
			return true;
		}
		return false;
	}

    public List<Wall> GetPerpendicularWalls()
    {
        if (orientation == Direction.NORTH || orientation == Direction.SOUTH)
        {
            return walls.FindAll(x => x.dir == Direction.EAST || x.dir == Direction.WEST);
        }
        return walls.FindAll(x => x.dir == Direction.NORTH || x.dir == Direction.SOUTH);
    }

	public bool OverlapsRegion(Region otherRegion)
	{
		return BoundsOverlap(bounds, otherRegion.bounds);
	}

	public Dictionary<Wall, Wall> GetOverlappingWalls(Region otherRegion)
	{
		Dictionary<Wall, Wall> overlappingWalls = new Dictionary<Wall, Wall>();
		for (int i = 0; i < this.walls.Count; i++)
		{
			for (int j = 0; j < otherRegion.walls.Count; j++)
			{
				Wall tw = this.walls[i];
				Wall ow = otherRegion.walls[j];

				// Check if walls overlap and aren't touching over the edge
				if (tw.OverlapsWall(ow) && !ArePerpendicual(tw.dir, ow.dir))
				{
					overlappingWalls.Add(tw, ow);
					//Debug.Log("Adding " + tw.dir + " & " + ow.dir);
				}
			}
		}
		return overlappingWalls;
	}

	public static Vector2Int GetPerpendicularDirectionVector(Direction dir)
	{
		return ((dir == Direction.NORTH || dir == Direction.SOUTH) 
			? new Vector2Int(1, 0) : new Vector2Int(0, 1));
	}
	public static Direction GetPerpendicularDirection(Direction dir)
	{
		return ((dir == Direction.NORTH || dir == Direction.SOUTH)
			? Direction.EAST : Direction.NORTH);
	}

	public static Vector2Int GetAbsoluteDirectionVector(Direction dir)
	{
		return ((dir == Direction.NORTH || dir == Direction.SOUTH)
			? new Vector2Int(0, 1) : new Vector2Int(1, 0));
	}

	public static Vector2Int GetDirectionVector(Direction dir)
	{
		return (
			(dir == Direction.NORTH) ? new Vector2Int(0, 1)  :	// North -> y + 1
			(dir == Direction.EAST) ? new Vector2Int(1, 0)   :	// East -> x + 1 
			(dir == Direction.SOUTH) ? new Vector2Int(0, -1) :	// South -> y - 1
			new Vector2Int(-1, 0)								// West -> x - 1
			);
	}
	public static Direction GetVectorDirection(Vector2Int dir)
	{
		return (
			(dir == new Vector2Int(0, 1)) ? Direction.NORTH  :  // North -> y + 1
			(dir == new Vector2Int(1, 0)) ? Direction.EAST   :  // East -> x + 1 
			(dir == new Vector2Int(0, -1)) ? Direction.SOUTH :  // South -> y - 1
			(dir == new Vector2Int(0, -1)) ? Direction.WEST  :	// West -> x - 1
			Direction.UNKOWN);   
	}

	public static Vector2 GetOppositeDirectionVector(Direction dir)
	{
		return (
			(dir == Direction.NORTH) ? new Vector2Int(0, -1) :  // South -> y - 1
			(dir == Direction.EAST) ? new Vector2Int(-1, 0)  :  // West -> x - 1
			(dir == Direction.SOUTH) ? new Vector2Int(0, 1)  :  // North -> y + 1
			(dir == Direction.WEST) ? new Vector2Int(1, 0)   :  // East -> x + 1 
			new Vector2Int(0, 0)
			);
	}
	public static Direction GetOppositeDirection(Direction dir)
	{
		return (
			(dir == Direction.NORTH) ? Direction.SOUTH :	// South -> y - 1
			(dir == Direction.EAST) ? Direction.WEST :		// West -> x - 1
			(dir == Direction.SOUTH) ? Direction.NORTH :	// North -> y + 1
			(dir == Direction.WEST) ? Direction.EAST :		// East -> x + 1 
			Direction.UNKOWN
			);
	}

	public override string ToString()
	{
		return ("[" + Id + ". " + orientation.ToString() + " | " + bounds.ToString() + "]");
	}
}