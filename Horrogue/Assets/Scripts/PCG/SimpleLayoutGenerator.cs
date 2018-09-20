using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SimpleLayoutGenerator : MonoBehaviour {

	public enum TileType
	{
		Air,
		Ground,
		Wall,
	}

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

	#region Public Variables
	public bool useRandomSeed;
	public string seed = "elementary";
	public BoundsInt generationBounds;
    public int tileSize = 1;
    public List<TileSprites> tileSprites = new List<TileSprites>()
    {
        new TileSprites(TileType.Air),
        new TileSprites(TileType.Ground),
        new TileSprites(TileType.Wall),
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
        int mainCorridorHalf = 3;
        int numSubCorridors = 2;

        // Pick random spot inside given bounds and extend a corridor in one direction to its maximum length
        // Also give the generation some padding so the corridor generates with it's full width
        Vector2Int mainCorridorSpawn = new Vector2Int(Random.Range(mainCorridorHalf, generationBounds.size.x - mainCorridorHalf), 
            Random.Range(mainCorridorHalf, generationBounds.size.y - mainCorridorHalf));

        Vector2Int[] subSpawns = new Vector2Int[numSubCorridors];
        BoundsInt[] corridors = new BoundsInt[numSubCorridors + 1];

        // Temporary direction choosing, 0 is horizontal, 1 is vertical
        bool isVertical = Mathf.RoundToInt(Random.value) == 1;

        // Determine bounds for main corridor
        int aMin = 0;
        int aMax = 0;
        int bMin = 0;
        int bMax = 0;
        

        for (int i = 0; i < numSubCorridors + 1; i++)
        {
            corridors[i] = new BoundsInt();
            if (i == 0) {
                corridors[i].xMin = (isVertical) ? mainCorridorSpawn.x - mainCorridorHalf 
                    : mainCorridorHalf;
                corridors[i].yMin = (!isVertical) ? mainCorridorSpawn.y - mainCorridorHalf 
                    : mainCorridorHalf;
                corridors[i].xMax = (isVertical) ? mainCorridorSpawn.x + mainCorridorHalf 
                    : generationBounds.size.x - mainCorridorHalf;
                corridors[i].yMax = (!isVertical) ? mainCorridorSpawn.y + mainCorridorHalf 
                    : generationBounds.size.y - mainCorridorHalf;

                isVertical = !isVertical;
            }
            else
            {
                corridors[i].xMin = (isVertical) ? mainCorridorSpawn.x - mainCorridorHalf 
                    : mainCorridorHalf;
                corridors[i].yMin = (!isVertical) ? mainCorridorSpawn.y - mainCorridorHalf 
                    : mainCorridorHalf;
                corridors[i].xMax = (isVertical) ? mainCorridorSpawn.x + mainCorridorHalf 
                    : generationBounds.size.x - mainCorridorHalf;
                corridors[i].yMax = (!isVertical) ? mainCorridorSpawn.y + mainCorridorHalf 
                    : generationBounds.size.y - mainCorridorHalf;
            }
            if (isVertical)
            {
                subSpawns[i] = new Vector2Int(mainCorridorSpawn.x, Random.Range(mainCorridorHalf, generationBounds.size.y - mainCorridorHalf));
            }
            else
            {
                subSpawns[i] = new Vector2Int(Random.Range(mainCorridorHalf, generationBounds.size.x - mainCorridorHalf), mainCorridorSpawn.y);
            }


            //if (isVertical)
            //{
            //    aMin = mainCorridorSpawn.x - mainCorridorHalf;
            //    aMax = mainCorridorSpawn.x + mainCorridorHalf;

            //    bMin = mainCorridorHalf;
            //    bMax = generationBounds.size.y - mainCorridorHalf;
            //}
            //else
            //{
            //    aMin = mainCorridorHalf;
            //    aMax = generationBounds.size.x - mainCorridorHalf;

            //    bMin = mainCorridorSpawn.y - mainCorridorHalf;
            //    bMax = mainCorridorSpawn.y + mainCorridorHalf;
            //}


        }

        for (int x = aMin; x < aMax; x++)
        {
            for (int y = bMin; y < bMax; y++)
            {
                TileType tile = TileType.Ground;
                if (x == aMin || x == aMax - 1 || y == bMin || y == bMax - 1)
                {
                    tile = TileType.Wall;
                }
                map[x, y] = (int) tile;
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
