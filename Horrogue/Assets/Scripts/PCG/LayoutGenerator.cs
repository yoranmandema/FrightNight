using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;



public class LayoutGenerator : MonoBehaviour {

	#region Structs
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
	#endregion

	#region Public Variables
	[Header("General")]
	public bool useRandomSeed = false;	// Should a seed be generated
	public string seed = "elementary";  // The current seed used for generation

	// The bounds where and how big the region generation is going to be
	public BoundsInt generationBounds = new BoundsInt(-50, -50, 0, 100, 100, 1);

    // How many times the generator should try placing content before skipping to the next step
    public int maxGenerationAttempts = 25;

	// Premade objects and regions
	[Header("Custom Content")]
	public List<CustomRegion> customRegions;

	[Header("Spawning Behaviour")]
	public int additionalCorridorAmount = 2;

    public int spawnAreaWidth = 10;
    public int spawnAreaHeight = 6;

    //public Range spawnAreaSize;

    public int mainCorridorWidth = 16;
    public int mainCorridorHeight = 44;

	public Range corridorWidth;
	public Range corridorLength;

	public Range classAreaSize;


    // Tile prefabs and settings
    [Header("Generator Tiles")]
	public int tileSize = 1;
    public List<TileSprites> tileSprites;
	#endregion

	#region Private Variables
    private List<Region> regions;
    private List<Region> corridors;
	private int[,] map;
    private Furniture[,,] furnituremap;
    private GameObject[,] tilemap;
    private GameObject parent;

    Region spawnRegion, mainCorridor;
    int numAddCor = 0, numAddReg = 0;
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

        SetupSpawn();

        CreateAdditionalCorridors();
        GenerateRegions();
        PlaceRegionContent();

        CreateTileMap();
	}

	private void PlaceRegionContent()
    {

    }

	private void CreateAdditionalCorridors()
	{
		for (int i = numAddCor; numAddCor < additionalCorridorAmount;)
        {
            if (!CreateRandomCorridor())
                i++;

            if (i >= maxGenerationAttempts)
            {
                Debug.LogWarning("Max iterations reached! Skipping additional corridor placement.");
                break;
            }
        }
        Debug.Log("Number of additional corridors: " + numAddCor);
	}

    private bool CreateRandomCorridor()
    {
        /** To create a random corrdior iterate over all existing corridors ...
         * 1 Take a random existing corridor
         * 1.1 Check if corridor has "free" walls perpendicual to it's orientation
         * 1.1.t Pick random "free" wall and carry on
         * 1.1.f Try another corridor (Step 1)
         * 2 Pick random spot on wall
         * 2.1 Check if minimum width (TODO + spacing) is possible
         * 2.1.t Get possible maximum width (TODO + spacing)
         * 2.1.f Try another random spot (Step 2)
         * 2.2 Check if minimum length is possible
         * 2.2.t Get possible maximum length
         * 2.2.f Try another random spot (Step 2)
         * 3 Create Corridor Region
         * */
        List<Region> cors = corridors;

        // New corridor params
        int cMaxW = corridorWidth.max, cMaxL = corridorLength.max;

        while (cors.Count > 0)
        {
            // Get and remove random corridor from list to avoid duplicate checking
            int cIndex = Random.Range(0, cors.Count);
            Region c = cors[cIndex];
            cors.RemoveAt(cIndex);

            // Create a list of all possible walls
            List<Region.Wall> walls = c.GetPerpendicularWalls();

            // Test corridor min width against height or against width of wall
            Vector2Int rotation = (c.orientation == Region.Direction.NORTH || c.orientation == Region.Direction.SOUTH) ?
                new Vector2Int(0, 1) : new Vector2Int(1, 0);
            while (walls.Count > 0)
            {
                // Get and remove random wall from list
                int wIndex = Random.Range(0, walls.Count);
                Region.Wall w = walls[wIndex];
                walls.RemoveAt(wIndex);

                // Quick access parameters
                Vector3Int wSize = w.bounds.size;

                // Test if there is enough space for the smallest possible corridor
                if (wSize.x * rotation.x >= corridorWidth.min || wSize.y * rotation.y >= corridorWidth.min)
                {
                    // Add all spots to a list
                    List<Vector3Int> spots = new List<Vector3Int>();
                    do {
                        // Avoid any spots too close to the edge for proper corridors
                        Vector3Int spot = w.bounds.allPositionsWithin.Current;
                        Vector3Int minPos = w.bounds.min, maxPos = w.bounds.max;
                        //if (true)
                        spots.Add(spot);
                        Debug.Log(spot);
                    } while (w.bounds.allPositionsWithin.MoveNext());

                    while (spots.Count > 0)
                    {
                        // Get and remove random spot from list
                        int sIndex = Random.Range(0, spots.Count);
                        Vector3Int s = spots[sIndex];
                        spots.RemoveAt(sIndex);

                        // Test

                    }
                }
            }
        }

        return false;
    }

    private Vector3Int GetPositionRelativeToRandomWall(Region region, Vector3Int size)
    {
        // TODO test for random PLAUSIBLE wall

        Region.Wall w = region.walls.Find(x => x.dir == Region.Direction.EAST);

        Debug.Log(w.bounds.center);
        return Vector3Int.RoundToInt(w.bounds.center - Vector3.Scale(size, new Vector3(0, .5f)));
    }

    private Vector3Int GetRandomPositionInsideBounds(Vector3Int targetSize)
	{
		Vector2 rand = new Vector2(Random.value, Random.value);
		int x = Mathf.RoundToInt(rand.x * (generationBounds.size.x - targetSize.x) + generationBounds.position.x);
		int y = Mathf.RoundToInt(rand.y * (generationBounds.size.y - targetSize.y) + generationBounds.position.y);

		return new Vector3Int(x, y, 0);
	}

	private void GenerateRegions()
    {
        
    }

	private void SetupSpawn()
    {
        for (int i = 0; i < customRegions.Count; i++)
        {
            CustomRegion reg = customRegions[i];
            BoundsInt bounds = new BoundsInt(Vector3Int.RoundToInt(reg.bounds.position + generationBounds.center 
                + Vector3.Scale(generationBounds.size, new Vector3(.5f, .5f))), reg.bounds.size);

			regions.Add(new Region(bounds, reg.type));
        }

        // Find Spawn Region or create a new one
        spawnRegion = regions.Find(x => x.isSpawn);
        if (spawnRegion == null)
        {
            Vector3Int size = new Vector3Int(spawnAreaWidth, spawnAreaHeight, 1);
            Vector3Int position = Vector3Int.RoundToInt(generationBounds.center - new Vector3(size.x / 2f, size.y / 2f));
            spawnRegion = new Region(new BoundsInt(position, size), RegionType.Toilet);

            //FIXME Remove
            regions.Add(spawnRegion);
        }
        else
        {
            regions.Remove(spawnRegion);
        }

        // Find Main Corridor or create a new one
        mainCorridor = regions.Find(x => x.type == RegionType.MainCorridor);
        if (mainCorridor == null)
        {
            Vector3Int size = new Vector3Int(mainCorridorWidth, mainCorridorHeight, 1);
            Region.Wall w = spawnRegion.walls.Find(x => x.dir == Region.Direction.EAST);
            Vector3Int position = Vector3Int.RoundToInt(w.bounds.center - Vector3.Scale(size, new Vector3(0, .5f)));
            mainCorridor = new Region(new BoundsInt(position, size), RegionType.MainCorridor);

            //FIXME Remove
            corridors.Add(mainCorridor);
        }
        else
        {
            regions.Remove(mainCorridor);
        }
    }

    private void CreateTileMap()
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

        regions = new List<Region>();
        corridors = new List<Region>();

        map = new int[generationBounds.size.x, generationBounds.size.y];
        tilemap = new GameObject[generationBounds.size.x, generationBounds.size.y];

        for (int x = 0; x < generationBounds.size.x; x++)
        {
            for (int y = 0; y < generationBounds.size.y; y++)
            {
                map[x, y] = (int)TileType.Air;
                tilemap[x, y] = null;
            }
        }
    }


    private void OnDrawGizmos()
    {
        if (generationBounds != null)
        {
            if (map != null)
            {
                for (int x = 0; x < generationBounds.size.x; x++)
                {
                    for (int y = 0; y < generationBounds.size.y; y++)
                    {
                        Vector3 position = new Vector3(generationBounds.xMin + (x + 0.5f) * tileSize,
                            generationBounds.yMin + (y + 0.5f) * tileSize);
                        Vector3 size = Vector3.one * tileSize;
                        TileType tile = (TileType)map[x, y];
                        switch (tile)
                        {
                            case TileType.Air:
                                Gizmos.color = Color.grey;
                                break;
                            case TileType.Wall:
                                Gizmos.color = Color.black;
                                break;
                            case TileType.Ground:
                                Gizmos.color = Color.green;
                                break;
                            default: break;
                        }
                        Gizmos.DrawCube(position, size);
                    }
                }
            }

            Gizmos.color = Color.green;

            Gizmos.DrawLine(new Vector3(generationBounds.xMin, generationBounds.yMin), new Vector3(generationBounds.xMin, generationBounds.yMax));
            Gizmos.DrawLine(new Vector3(generationBounds.xMin, generationBounds.yMin), new Vector3(generationBounds.xMax, generationBounds.yMin));
            Gizmos.DrawLine(new Vector3(generationBounds.xMax, generationBounds.yMax), new Vector3(generationBounds.xMin, generationBounds.yMax));
            Gizmos.DrawLine(new Vector3(generationBounds.xMax, generationBounds.yMax), new Vector3(generationBounds.xMax, generationBounds.yMin));
            

            if (regions != null && regions.Count > 0)
            {
                foreach (Region r in regions)
                {
                    Gizmos.DrawLine(new Vector3(r.bounds.xMin, r.bounds.yMin), new Vector3(r.bounds.xMin, r.bounds.yMax));
                    Gizmos.DrawLine(new Vector3(r.bounds.xMin, r.bounds.yMin), new Vector3(r.bounds.xMax, r.bounds.yMin));
                    Gizmos.DrawLine(new Vector3(r.bounds.xMax, r.bounds.yMax), new Vector3(r.bounds.xMin, r.bounds.yMax));
                    Gizmos.DrawLine(new Vector3(r.bounds.xMax, r.bounds.yMax), new Vector3(r.bounds.xMax, r.bounds.yMin));
                }
            }
        }
    }
}
