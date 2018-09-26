using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// Types of tiles that are available
public enum TileType
{
	Air,
	Ground,
	Wall,
    Doorway
}


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

// Container for sprites to be used for a certain tile type
[Serializable]
public struct TileSprites
{
	public TileSprites(TileType type)
	{
		this.tileType = type;
		this.tilePrefabs = new List<GameObject>(1);
	}

	public TileType tileType;
	public List<GameObject> tilePrefabs;
}

[Serializable]
public struct Object
{
    public string name;
    public GameObject prefab;
    public Direction facing;
}

// Container for regions that are placed beforehand
[Serializable]
public struct PremadeRegion
{
	public PremadeRegion(Vector3 center)
	{
		this.name = "New Region";
		this.type = RegionType.None;
		Vector3Int bottomLeft = Vector3Int.RoundToInt(center) - new Vector3Int(5, 5, 0);
		this.bounds = new BoundsInt(bottomLeft, new Vector3Int(10, 10, 1));
        this.usePrefab = false;
        this.prefab = null;
	}

	public string name;
	public RegionType type;
	public BoundsInt bounds;
    public bool usePrefab;
    public GameObject prefab;
}

[Serializable]
public struct Wall
{
    public Direction facing;
    public BoundsInt bounds;

    public Wall(Direction facing, BoundsInt bounds)
    {
        this.facing = facing;
        this.bounds = bounds;
    }
}

[Serializable]
public struct Region
{
    public bool isConnected;
    public List<Region> connectedRooms;
    public Wall[] walls;
    public BoundsInt bounds;
    public RegionType type;

    public Region(BoundsInt bounds, RegionType type)
    {
        this.isConnected = false;
        this.connectedRooms = new List<Region>();
        this.walls = new Wall[4];
        this.bounds = bounds;
        this.type = type;
        GenerateWallsFromBounds();
    }

    public Wall GetWall(Direction dir)
    {
        return new Wall();
    }

    private void GenerateWallsFromBounds()
    {
        for (int i = 0; i < 4; i++)
        {

        }
    }
}

[Serializable]
public struct Range
{
	public int min;
	public int max;
	
	public Range(int min, int max)
	{
		this.min = min;
		this.max = max;
	}
}

[Serializable]
public struct SpawnOptions
{
	public RegionType type;
	public bool enableSpawning;
	public Range amount;
	public Range width;
	public Range length;

	public SpawnOptions(RegionType type)
	{
		this.type = type;
		this.enableSpawning = false;
		this.amount = new Range(0, 1);
		this.width = new Range(0, 1);
		this.length = new Range(0, 1);
	}
}

// Directions
[Serializable]
public class Direction
{
    public static Vector2Int 
        NORTH = new Vector2Int(0, 1),
        WEST = new Vector2Int(1, 0),
        SOUTH = new Vector2Int(0, -1),
        EAST = new Vector2Int(-1, 0);
}

public class SimpleLayoutGenerator : MonoBehaviour {

	#region Public Variables
	[Header("General")]
	public bool useRandomSeed = false;	// Should a seed be generated
	public string seed = "elementary";  // The current seed used for generation

	// The bounds where and how big the region generation is going to be
	public BoundsInt generationBounds = new BoundsInt(-50, -50, 0, 100, 100, 1);

	// Premade objects and regions
	[Header("Custom Content")]
	public List<PremadeRegion> premadeRegions;

	[Header("Spawning Behaviour")]
	public bool spawnSpecialRooms = true;
    //public int wallThickness = 1;
    public SpawnOptions corridorRegions;
    public SpawnOptions classRegions;
    public SpawnOptions facultyRegions;
    public SpawnOptions outsideRegions;
    public SpawnOptions administrativeRegions;
    public SpawnOptions utilityRegions;


    // Tile prefabs and settings
    [Header("Generator Tiles")]
	public int tileSize = 1;
    public List<TileSprites> tileSprites;
    #endregion

    #region Private Variables
    private Region regions;
	private int[,] map;
    private Object[,,] objectmap;
    private GameObject[,] tilemap;
    private GameObject parent;
    #endregion

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GenerateLayout();
        }
    }

    public void GenerateLayout()
	{
		InitializeRandom();
		InitializeMap();

        SetupCustomRegions();
        GenerateRandomRegions(false);

        ConnectRegions();

        GenerateRandomRegions(true);

        PlaceRegionContent();

        PlaceTiles();
	}

    private void PlaceRegionContent()
    {
        throw new NotImplementedException();
    }

    private void GenerateRandomRegions(bool forceConnection)
    {
        throw new NotImplementedException();
    }

    private void ConnectRegions()
    {
        throw new NotImplementedException();
    }

    private void SetupCustomRegions()
    {
        for (int i = 0; i < premadeRegions.Count; i++)
        {
            PremadeRegion reg = premadeRegions[i];
            BoundsInt bounds = new BoundsInt(Vector3Int.RoundToInt(reg.bounds.position + generationBounds.center 
                + Vector3.Scale(generationBounds.size, new Vector3(.5f, .5f))), reg.bounds.size);

            regions = new Region(false, )

            
        }
    }

    private void PlaceTiles()
    {
        for (int x = 0; x < generationBounds.size.x; x++)
        {
            for (int y = 0; y < generationBounds.size.y; y++)
            {
                Vector3 position = new Vector3(generationBounds.xMin + (x + 0.5f) * tileSize,
                    generationBounds.yMin + (y + 0.5f * tileSize) * tileSize);
                Vector3 size = Vector3.one * tileSize;

                GameObject tilePrefab; 
                TileType tileType = (TileType)map[x, y];
                tilePrefab = tileSprites[(int)tileType].tilePrefabs[0];

                tilemap[x, y] = Instantiate(tilePrefab, position, Quaternion.identity, parent.transform);
            }
        }
    }

    private void InitializeRandom()
	{
		if (useRandomSeed)
		{
			seed = System.DateTime.Now.Ticks.ToString();
		}
		Random.InitState(seed.GetHashCode());
	}

	private void InitializeMap()
	{
        if (parent != null)
        {
            Destroy(parent);
            parent = null;
        }
        parent = new GameObject("Tile Map");
        map = new int[generationBounds.size.x, generationBounds.size.y];
		tilemap = new GameObject[generationBounds.size.x, generationBounds.size.y];
        for (int x = 0; x < generationBounds.size.x; x++)
		{
			for (int y = 0; y < generationBounds.size.y; y++)
			{
				map[x, y] = (int) TileType.Air;
				tilemap[x, y] = null;
            }
		}
	}

	private void MapPremadeRegions()
	{
        for (int i = 0; i < premadeRegions.Count; i++)
        {
            PremadeRegion reg = premadeRegions[i];
            BoundsInt bounds = new BoundsInt(Vector3Int.RoundToInt(reg.bounds.position + generationBounds.center + Vector3.Scale(generationBounds.size, new Vector3(.5f, .5f))), reg.bounds.size);
            Debug.Log(bounds);
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    TileType tile = TileType.Ground;
                    if ((TileType)map[x, y] != TileType.Ground && (x == bounds.xMin || x == bounds.xMax - 1 || y == bounds.yMin || y == bounds.yMax - 1))
                    {
                        tile = TileType.Wall;
                    }
                    map[x, y] = (int) tile;
                }
            }
        }

	}

	private void MapCorridors()
	{

	}

    private void MapCorridorsOld()
    {
        // First place a main corridor if none are placed
        // Some temporary parameters
        int mainCorridorHalf = 2;
        int numSubCorridors = 4;

        // Pick random spot inside given bounds and extend a corridor in one direction to its maximum length
        // Also give the generation some padding so the corridor generates with it's full width
        Vector2Int mainCorridorSpawn = new Vector2Int(Random.Range(mainCorridorHalf, generationBounds.size.x - mainCorridorHalf), 
            Random.Range(mainCorridorHalf, generationBounds.size.y - mainCorridorHalf));

        BoundsInt[] corridors = new BoundsInt[numSubCorridors + 1];

        // Temporary direction choosing, 0 is horizontal, 1 is vertical
        bool isVertical = Mathf.RoundToInt(Random.value) == 1;

		Vector2Int corridorSpawn = new Vector2Int();
		for (int i = 0; i < numSubCorridors + 1; i++)
        {
			// Place the main corridor first
			// Then determine if the other corridors are supposed to be horizontal or vertical to be orthogonal
			// to the first (main) corridor
			if (i == 0)
			{
				corridorSpawn = mainCorridorSpawn;
			}
			else if (isVertical)
			{
				corridorSpawn.x = Random.Range(mainCorridorHalf, generationBounds.size.x - mainCorridorHalf);
				corridorSpawn.y = mainCorridorSpawn.y;
			}
			else
			{
				corridorSpawn.x = mainCorridorSpawn.x;
				corridorSpawn.y = Random.Range(mainCorridorHalf, generationBounds.size.y - mainCorridorHalf);
			}

			// Calculate the corridor bounds
			corridors[i] = new BoundsInt();
			corridors[i].xMin = (isVertical) ? corridorSpawn.x - mainCorridorHalf
					: mainCorridorHalf;
			corridors[i].yMin = (!isVertical) ? corridorSpawn.y - mainCorridorHalf
				: mainCorridorHalf;
			corridors[i].xMax = (isVertical) ? corridorSpawn.x + mainCorridorHalf
				: generationBounds.size.x - mainCorridorHalf;
			corridors[i].yMax = (!isVertical) ? corridorSpawn.y + mainCorridorHalf
				: generationBounds.size.y - mainCorridorHalf;

			if (i == 0) {
				isVertical = !isVertical;
            }

			for (int x = corridors[i].xMin; x <= corridors[i].xMax; x++)
			{
				for (int y = corridors[i].yMin; y <= corridors[i].yMax; y++)
				{
					TileType tile = TileType.Ground;
					if (map[x,y] != (int)TileType.Ground && (x == corridors[i].xMin || x == corridors[i].xMax || y == corridors[i].yMin || y == corridors[i].yMax))
					{
						tile = TileType.Wall;
					}
					map[x, y] = (int)tile;
				}
			}
		}
    }

    private void MapRooms()
	{
		
	}


    private void OnDrawGizmos()
    {
        if (generationBounds != null)
        {
            
            Gizmos.color = Color.green;

            Gizmos.DrawLine(new Vector3(generationBounds.xMin, generationBounds.yMin), new Vector3(generationBounds.xMin, generationBounds.yMax));
            Gizmos.DrawLine(new Vector3(generationBounds.xMin, generationBounds.yMin), new Vector3(generationBounds.xMax, generationBounds.yMin));
            Gizmos.DrawLine(new Vector3(generationBounds.xMax, generationBounds.yMax), new Vector3(generationBounds.xMin, generationBounds.yMax));
            Gizmos.DrawLine(new Vector3(generationBounds.xMax, generationBounds.yMax), new Vector3(generationBounds.xMax, generationBounds.yMin));

            //if (map != null)
            //{
            //    for (int x = 0; x < generationBounds.size.x; x++)
            //    {
            //        for (int y = 0; y < generationBounds.size.y; y++)
            //        {
            //            Vector3 position = new Vector3(generationBounds.xMin + (x + 0.5f) * tileSize,
            //                generationBounds.yMin + (y + 0.5f) * tileSize);
            //            Vector3 size = Vector3.one * tileSize;
            //            TileType tile = (TileType)map[x, y];
            //            switch (tile)
            //            {
            //                case TileType.Air:
            //                    Gizmos.color = Color.grey;
            //                    break;
            //                case TileType.Wall:
            //                    Gizmos.color = Color.black;
            //                    break;
            //                case TileType.Ground:
            //                    Gizmos.color = Color.green;
            //                    break;
            //                default: break;
            //            }
            //            Gizmos.DrawCube(position, size);
            //        }
            //    }
            //}
        }
    }
}
