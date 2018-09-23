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
}


// Types of regions that can be generated
public enum RegionType
{
	None,
	MainCorridor,
	Corridor,
	Cafeteria,
	Nursery,
	MusicRoom,
	ArtRoom,
	SmallRoom,
	MediumRoom,
	LargeRoom,
	Storage,
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
	}

	public string name;
	public RegionType type;
	public BoundsInt bounds;
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

public class SimpleLayoutGenerator : MonoBehaviour {

	#region Public Variables
	// Generation options
	public bool useRandomSeed = false;	// Should a seed be generated
	public string seed = "elementary";  // The current seed used for generation

	// The bounds where and how big the region generation is going to be
	public BoundsInt generationBounds = new BoundsInt(-50, -50, 0, 100, 100, 1);

	// Premade objects and regions
	public List<PremadeRegion> premadeRegions;

	public bool spawnCorridors = true;
	public Range corridorAmount;
	public Range corridorWidth;
	public Range corridorLength;

	public bool spawnSpecialRooms = true;
	public bool spawnSmallRooms = true;
	public Range smallRoomAmount;
	public Range smallRoomWidth;
	public Range smallRoomHeight;

	public bool spawnMediumRooms = true;
	public Range mediumRoomAmount;
	public Range mediumRoomWidth;
	public Range mediumRoomHeight;

	public bool spawnLargeRooms = true;
	public Range largeRoomAmount;
	public Range largeRoomWidth;
	public Range largeRoomHeight;

	// Tile prefabs and settings
	public int tileSize = 1;
    public List<TileSprites> tileSprites = new List<TileSprites>(3)
    {
        new TileSprites(TileType.Air),
        new TileSprites(TileType.Ground),
        new TileSprites(TileType.Wall)
    };
	#endregion

	#region Private Variables
	private int[,] map;
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

		MapCorridors();
		MapRooms();

        PlaceTiles();
	}

    private void PlaceTiles()
    {
        for (int x = 0; x < generationBounds.size.x; x++)
        {
            for (int y = 0; y < generationBounds.size.y; y++)
            {
                Vector3 position = new Vector3(generationBounds.xMin + (x + 0.5f) * tileSize,
                    generationBounds.yMin + (y + 0.5f) * tileSize);
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

    private void MapCorridors()
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
