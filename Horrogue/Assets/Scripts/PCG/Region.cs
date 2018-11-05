using System;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

// Types of regions that can be generated
public enum RegionType
{
	None,
	MainCorridor,
	Corridor,
	Toilets,
	ClassRoom,
	Storage,
	PrincipalsOffice
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
	public List<BoundsInt> possibleConnections;
	public int thickness;

	public static int thicknessNorth = 2;
	public static int thicknessWest = 1;
	public static int thicknessSouth = 1;
	public static int thicknessEast = 1;

	public Wall(Direction dir, BoundsInt bounds) : this(dir, bounds, new List<BoundsInt>())
	{ }	
	public Wall(Direction dir, BoundsInt bounds, List<BoundsInt> possibleBounds)
	{
		this.dir = dir;
		this.bounds = bounds;
		this.possibleConnections = new List<BoundsInt>(possibleBounds);

		switch (dir)
		{
			case Direction.NORTH:
				isVertical = false;
				thickness = thicknessNorth;
				break;
			case Direction.EAST:
				isVertical = true;
				thickness = thicknessEast;
				break;
			case Direction.SOUTH:
				isVertical = false;
				thickness = thicknessSouth;
				break;
			case Direction.WEST:
				isVertical = true;
				thickness = thicknessWest;
				break;
			default:
				throw new Exception("Unknown wall direction specified!");
		}

		ScalePossibleConnections();
	}

	private void ScalePossibleConnections()
	{
		for (int i = 0; i < possibleConnections.Count; i++) {
			BoundsInt pCon = possibleConnections[i];
			switch (dir)
			{
				case Direction.EAST:
				case Direction.NORTH:
					pCon.size += pCon.size * GetDirectionThickness(dir);
					break;

				case Direction.SOUTH:
				case Direction.WEST:
					pCon.min += pCon.size * GetDirectionThickness(dir);
					break;

				default:
					throw new Exception("Unknown wall direction! '" + dir.ToString() + "'");
			}
			possibleConnections[i] = pCon;
		}
	}

	public static Vector3Int GetDirectionThickness(Direction dir)
	{
		switch (dir)
		{
			case Direction.NORTH:
				return new Vector3Int(0, thicknessNorth, 0);
			case Direction.EAST:
				return new Vector3Int(thicknessEast, 0, 0);
			case Direction.SOUTH:
				return new Vector3Int(0, -thicknessSouth, 0);
			case Direction.WEST:
				return new Vector3Int(-thicknessWest, 0, 0);
			default:
				throw new Exception("Unknown direction specified!");
		}
	}

	public bool OverlapsWall(Wall otherWall)
	{
		Bounds thisBounds = new Bounds(bounds.center, bounds.size);
		Bounds otherBounds = new Bounds(otherWall.bounds.center, otherWall.bounds.size);
		//Debug.Log("This [" + dir.ToString() + " -> " + thisBounds.ToString() + "]\nOther [" + otherWall.dir.ToString() + " -> " + otherBounds.ToString() + "]");
		//Debug.Log(thisBounds.Intersects(otherBounds));
		return (thisBounds.Intersects(otherBounds) || otherBounds.Intersects(thisBounds));
	}

	public BoundsInt GetRandomConnection()
	{
		BoundsInt connection = possibleConnections[Random.Range(0, possibleConnections.Count)];
		return connection;
	}

	public bool RemoveConnection(BoundsInt connection)
	{
		return possibleConnections.Remove(connection);
	}

	public override string ToString()
	{
		return ("[" + dir + " | " + bounds.ToString() + "]");
	}
}

[Serializable]
public class Region
{
	public static int NEXT_ID = 0;
	private int id;

	public bool isSpawn = false;
	public bool isConnected = false;
	public bool isFurnished = false;

	public List<Region> connectedRegions = new List<Region>();
	public List<BoundsInt> connections = new List<BoundsInt>();
	public List<Wall> walls;
	public List<RegionFurnitures> furnitures;
	public List<VariableRegionFurnitures> variableFurnitures;

	public List<GameObject> placedFurnitures = new List<GameObject>();

	public BoundsInt outerBounds;
	public BoundsInt innerBounds;

	public RegionType type;
	public Direction orientation;

	public Tileset floorTiles;
	public Tileset wallTiles; // Todo

	public const int cornerThreshold = 1; // may change to wall thickness, since it determines corner size
	public const int minWallWidth = 2;

	private Direction lastConDir;

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

		this.outerBounds = bounds;
		this.type = type;
		this.orientation = orientation;

		GenerateWallsFromBounds();
	}

	public Region(VariantRegion variantRegion, Vector3Int position) 
		: this((PremadeRegion) variantRegion, position)
	{
		this.variableFurnitures = new List<VariableRegionFurnitures>(variantRegion.variableFurnitures);
	}

	public Region(PremadeRegion premadeRegion, Vector3Int position)
	{
		id = NEXT_ID++;

		this.innerBounds = new BoundsInt(position.x, position.y, 0, premadeRegion.innerRegionWidth, premadeRegion.innerRegionLength, 1);

		this.type = premadeRegion.type;

		this.furnitures = new List<RegionFurnitures>(premadeRegion.furnitures);
		if (this.variableFurnitures == null)
		{
			this.variableFurnitures = new List<VariableRegionFurnitures>();
		}

		this.floorTiles = premadeRegion.tileset;

		GenerateWallsFromInnerBounds(premadeRegion.connections);
	}

	private void GenerateWallsFromInnerBounds(List<RegionConnections> possibleConnections)
	{
		List<Direction> directions = new List<Direction>()
		{
			Direction.NORTH,
			Direction.EAST,
			Direction.SOUTH,
			Direction.WEST
		};

		BoundsInt outerBounds = new BoundsInt(innerBounds.min, innerBounds.size);
		walls = new List<Wall>();
		for (int i = 0; i < directions.Count; i++)
		{
			Direction wallDir = directions[i];

			RegionConnections conns = possibleConnections.Find(x => x.direction == wallDir);

			List<BoundsInt> newPossibleConnections = new List<BoundsInt>();

			for (int j = 0; conns != null && j < conns.boundsList.Count; j++)
			{
				BoundsInt newConnection = new BoundsInt(conns.boundsList[j].position + Vector3Int.RoundToInt(innerBounds.center), conns.boundsList[j].size);
				newPossibleConnections.Add(newConnection);
			}

			BoundsInt wallBounds;
			switch (wallDir)
			{
				case Direction.NORTH:
					wallBounds = new BoundsInt(innerBounds.x, innerBounds.yMax, 0, innerBounds.size.x, Wall.thicknessNorth, 1);
					outerBounds.yMax += Wall.thicknessNorth;
					break;
				case Direction.EAST:
					wallBounds = new BoundsInt(innerBounds.xMax, innerBounds.y, 0, Wall.thicknessEast, innerBounds.size.y, 1);
					outerBounds.xMax += Wall.thicknessWest;
					break;
				case Direction.SOUTH:
					wallBounds = new BoundsInt(innerBounds.x, innerBounds.y - Wall.thicknessSouth, 0, innerBounds.size.x, Wall.thicknessSouth, 1);
					outerBounds.yMin -= Wall.thicknessSouth;
					break;
				case Direction.WEST:
					wallBounds = new BoundsInt(innerBounds.x - Wall.thicknessWest, innerBounds.y, 0, Wall.thicknessWest, innerBounds.size.y, 1);
					outerBounds.xMin -= Wall.thicknessWest;
					break;
				default:
					throw new Exception("Unknown wall direction! '" + wallDir.ToString() + "'");
			}

			Wall newWall = new Wall(wallDir, wallBounds, newPossibleConnections);
			walls.Add(newWall);
		}

		this.outerBounds = outerBounds;
	}

	public void ConnectToRegion(Region otherRegion)
	{
		// Find the overlapping connections
		Wall[] overlappingWalls = GetOverlappingWalls(otherRegion);
		BoundsInt[] overlappingConnections = GetOverlappingConnections(overlappingWalls);

		// Calculate the connection bounds
		BoundsInt connectionBounds = CalculateConnectionBounds(overlappingConnections[0], overlappingConnections[1], lastConDir);
		Debug.Log("Connect To Region - " + "Connection Bounds: " + connectionBounds.ToString());

		// Add other region to connected regions
		if (connectedRegions.Find(x=> x == otherRegion) == null)
			connectedRegions.Add(otherRegion);

		// Check if this region or another region is spawn or connected to spawn
		isConnected = (isSpawn || otherRegion.connectedRegions.Find(x => x.isConnected || x.isSpawn) != null);

		// Add this region to the connected regions of the other region
		if (otherRegion.connectedRegions.Find(x => x == this) == null)
			otherRegion.connectedRegions.Add(this);
		otherRegion.isConnected = isConnected;

		CreateConnection(otherRegion, connectionBounds, overlappingWalls, overlappingConnections);
	}

	private void CreateConnection(Region otherRegion, BoundsInt connectionBounds, Wall[] overlappingWalls, BoundsInt[] overlappingConnections)
	{
		// Add connection bounds to both regions
		connections.Add(connectionBounds);
		otherRegion.connections.Add(connectionBounds);

		// Remove overlapping connections and any other connections from overlapping walls
		// to prevent unnescessarry overlap testing later on
		
		Wall a = overlappingWalls[0],
			b = overlappingWalls[1];

		RemoveBlockedConnections(a, b);
		otherRegion.RemoveBlockedConnections(b, a);
	}

	private void RemoveBlockedConnections(Wall a, Wall b)
	{
		int index = walls.IndexOf(a);
		List<BoundsInt> possibleConnections = new List<BoundsInt>();
		foreach (BoundsInt connection in a.possibleConnections)
		{
			if (BoundsOverlap(connection, b.bounds))
			{
				Debug.LogWarning("Remove Blocked Connection - " + " Removing Connection: " + connection.ToString() + " (overlap with " + b.bounds.ToString());
			}
			else
			{
				possibleConnections.Add(connection);
			}
		}
		Debug.Log("Remove Blocked Connection - " + "Connection Count: " + a.possibleConnections.Count + " -> " + possibleConnections.Count);
		a.possibleConnections = possibleConnections;
		walls[index] = a;
	}

	private BoundsInt CalculateConnectionBounds(BoundsInt a, BoundsInt b, Direction direction)
	{
		BoundsInt bounds = new BoundsInt();
		switch (direction)
		{
			case Direction.NORTH:
				bounds.yMin = a.yMin;
				bounds.yMax = b.yMax;

				if  (a.size.x > b.size.x)
				{
					bounds.xMin = b.xMin;
					bounds.xMax = b.xMax;
				}
				else
				{
					bounds.xMin = a.xMin;
					bounds.xMax = a.xMax;
				}
				break;
			case Direction.EAST:
				bounds.xMin = a.xMin;
				bounds.xMax = b.xMax;

				if (a.size.y > b.size.y)
				{
					bounds.yMin = b.yMin;
					bounds.yMax = b.yMax;
				}
				else
				{
					bounds.yMin = a.yMin;
					bounds.yMax = a.yMax;
				}
				break;
			case Direction.SOUTH:
				bounds.yMin = b.yMin;
				bounds.yMax = a.yMax;

				if (a.size.x > b.size.x)
				{
					bounds.xMin = b.xMin;
					bounds.xMax = b.xMax;
				}
				else
				{
					bounds.xMin = a.xMin;
					bounds.xMax = a.xMax;
				}
				break;
			case Direction.WEST:
				bounds.xMin = b.xMin;
				bounds.xMax = a.xMax;

				if (a.size.y > b.size.y)
				{
					bounds.yMin = b.yMin;
					bounds.yMax = b.yMax;
				}
				else
				{
					bounds.yMin = a.yMin;
					bounds.yMax = a.yMax;
				}
				break;
			default:
				throw new Exception("Unknown wall direction! '" + direction.ToString() + "'");
		}

		return bounds;
	}

	private BoundsInt[] GetOverlappingConnections(Region otherRegion)
	{
		// Get overlapping walls
		return GetOverlappingConnections(GetOverlappingWalls(otherRegion));
	}
	private BoundsInt[] GetOverlappingConnections(Wall[] overlappingWalls)
	{
		// Get overlapping connections
		BoundsInt[] overlappingConnections = new BoundsInt[2];
		for (int i = 0; i < overlappingWalls[0].possibleConnections.Count; i++)
		{
			// this connection
			BoundsInt tc = overlappingWalls[0].possibleConnections[i];

			for (int j = 0; j < overlappingWalls[1].possibleConnections.Count; j++)
			{
				// other connection
				BoundsInt oc = overlappingWalls[1].possibleConnections[j];

				if (BoundsOverlap(tc, oc))
				{
					overlappingConnections[0] = tc;
					overlappingConnections[1] = oc;
					Debug.Log("Get Overlapping Connections - " + "Adding " + tc.ToString() + " & " + oc.ToString());
					lastConDir = overlappingWalls[0].dir;
					return overlappingConnections;
				}
			}
		}
		throw new Exception("No overlapping connections found!");
	}

	private Wall[] GetOverlappingWalls(Region otherRegion)
	{

		// Get overlapping walls
		Wall[] overlappingWalls = new Wall[2];
		for (int i = 0; i < this.walls.Count; i++)
		{
			Wall tw = this.walls[i];
			for (int j = 0; j < otherRegion.walls.Count; j++)
			{
				Wall ow = otherRegion.walls[j];

				// Check if walls overlap and aren't touching over the edge
				if (tw.OverlapsWall(ow) && !ArePerpendicual(tw.dir, ow.dir) && tw.dir != ow.dir)
				{
					overlappingWalls[0] = tw;
					overlappingWalls[1] = ow;
					Debug.Log("Get Overlapping Walls - " + "Adding " + tw.ToString() + " & " + ow.ToString());
					return overlappingWalls;
				}
			}
		}

		throw new Exception("No overlapping walls found!");
	}

	// Returns wall overlap
	public BoundsInt ConnectToRegion (Region otherRegion, int connectionSize)
	{
		//Debug.Log("Connecting " + this.ToString() + " to " + otherRegion.ToString());
		Wall[] overlappingWalls = GetOverlappingWalls(otherRegion);
		
		// Take entire wall as bounds
		if (overlappingWalls.Length == 2)
		{
			Wall wallA = overlappingWalls[0], 
				wallB = overlappingWalls[1];
			BoundsInt connectionBounds = CalculateOverlap(wallA.bounds, wallB.bounds);
			int overlapSize = connectionBounds.size.x + connectionBounds.size.y - 3;
			
			if (connectionSize > 0 && connectionSize <= overlapSize)
			{
				Vector3Int pos, size;
				if (wallA.isVertical) // | Vertical
				{
					pos = Vector3Int.FloorToInt(connectionBounds.center) - new Vector3Int(0, connectionSize / 2, 0);
					size = new Vector3Int(1, connectionSize, 1);
				}
				else // __ Horizontal
				{
					pos = Vector3Int.FloorToInt(connectionBounds.center) - new Vector3Int(connectionSize / 2, 0, 0);
					size = new Vector3Int(connectionSize, 1, 1);
				}
				connectionBounds = new BoundsInt(pos, size);

			}

			ConnectToRegion(otherRegion, connectionBounds, wallA, wallB);
			return connectionBounds;
		}
		Debug.Log(overlappingWalls.Length);
		throw new Exception("Can't connect Regions! Either none or too many walls are overlapping!"); 
	}

	public void ConnectToRegion (Region otherRegion, BoundsInt connectionBounds, Wall thisWall, Wall otherWall)
	{
		// Add other region to connected regions
		connectedRegions.Add(otherRegion);

		// Check if this region or another region is spawn or connected to spawn
		isConnected = (isSpawn || otherRegion.connectedRegions.Find(x => x.isConnected || x.isSpawn) != null);

		// Add this region to the connected regions of the other region
		otherRegion.connectedRegions.Add(this);
		otherRegion.isConnected = isConnected;

		// Split walls and register connection for both regions
		CreateConnection(otherRegion, connectionBounds, thisWall, otherWall);
	}

	private void CreateConnection(Region otherRegion, BoundsInt connectionBounds, Wall thisWall, Wall otherWall)
	{
		// Add connection bounds to both regions
		connections.Add(connectionBounds);
		otherRegion.connections.Add(connectionBounds);

		// Get wall overlap
		BoundsInt overlap = CalculateOverlap(thisWall.bounds, otherWall.bounds);

		// Split walls
		SplitWall(this, thisWall, overlap);
		SplitWall(otherRegion, otherWall, overlap);
	}

	private void SplitWall(Region region, Wall wall, BoundsInt splitBounds)
	{
		Vector2Int dir = GetPerpendicularDirectionVector(wall.dir);
		Vector3Int corner = new Vector3Int(cornerThreshold * dir.x, cornerThreshold * dir.y, 0);
		Vector3Int offset = Vector3Int.one - corner;
		Wall wallA = wall, wallB = wall;

		region.walls.Remove(wall);
		
		//Debug.Log("OG Wall:" + wall.ToString() + "; Split Bounds: (min)" + splitBounds.min.ToString() + " | (max)" + splitBounds.max.ToString());
		//Debug.Log("Before: Wall A:" + wallA.ToString() + "; Wall B:" + wallB.ToString());

		wallA.bounds.max = splitBounds.min + Vector3Int.one - corner * 2;
		wallB.bounds.min = splitBounds.max - Vector3Int.one + corner * 2;

		//Debug.Log("After: Wall A:" + wallA.ToString() + "; Wall B:" + wallB.ToString());

		if ((wallA.isVertical && wallA.bounds.size.y > minWallWidth) || (!wallA.isVertical && wallA.bounds.size.x > minWallWidth))
		{
			region.walls.Add(wallA);
		}
		if ((wallB.isVertical && wallB.bounds.size.y > minWallWidth) || (!wallB.isVertical && wallB.bounds.size.x > minWallWidth))
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
		// Convert IntBounds to Bounds to use Bounds.Intersects()
		Vector3 overlapMargin = new Vector3(overlapThreshhold, overlapThreshhold),
			offset = new Vector3(0.5f, 0.5f);

		Bounds boundsA = new Bounds(a.center + offset + overlapMargin, a.size - overlapMargin * 2);
		Bounds boundsB = new Bounds(b.center + offset + overlapMargin, b.size - overlapMargin * 2);

		//Debug.Log(boundsA.min.ToString() + " and " + boundsB.min.ToString() + " are intersecting? " + boundsA.Intersects(boundsB));

		return (boundsA.Intersects(boundsB) || boundsB.Intersects(boundsA));
	}

	private void GenerateWallsFromBounds()
	{
		this.connections = new List<BoundsInt>();
		this.walls = new List<Wall>();

		Vector3Int pos = outerBounds.position;
		Vector3Int size = outerBounds.size;

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

	public static BoundsInt GetOuterBounds(Vector3Int position, Vector3Int size)
	{
		BoundsInt outerBounds = new BoundsInt(position, size);
		outerBounds.yMax += Wall.thicknessNorth;
		outerBounds.xMax += Wall.thicknessWest;
		outerBounds.yMin -= Wall.thicknessSouth;
		outerBounds.xMin -= Wall.thicknessWest;
		return outerBounds;
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
		return BoundsOverlap(outerBounds, otherRegion.outerBounds);
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

	public static Vector3Int GetDirectionVector(Direction dir)
	{
		return (
			(dir == Direction.NORTH) ? new Vector3Int(0, 1, 0)  :	// North -> y + 1
			(dir == Direction.EAST) ? new Vector3Int(1, 0, 0) :		// East -> x + 1 
			(dir == Direction.SOUTH) ? new Vector3Int(0, -1, 0) :	// South -> y - 1
			new Vector3Int(-1, 0, 0)                                // West -> x - 1
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
		return ("[" + Id + ". " + orientation.ToString() + " | " + outerBounds.ToString() + "]");
	}
}