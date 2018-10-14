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

		public override string ToString()
		{
			return ("[" + min + "|" + max + "]");
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
	public int additionalRegionAmount = 10;

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
	private RegionSpot[,] regionMap;

	private List<Region> corridors;
	private TileType[,] map;
    private Furniture[,,] furnituremap;
    private GameObject[,] tilemap;
    private GameObject parent;

    Region spawnRegion, mainCorridor;
    int numAddCor, numAddReg;

    BoundsInt DebugBounds = new BoundsInt();
    #endregion

    public void GenerateLayout()
	{
		InitializeRandom();
		InitializeMap();

        SetupSpawn();

        CreateAdditionalCorridors();
        GenerateRegions();
		CreateAdditionalRegions();
		RandomizeRegionContent();

        CreateTileMap();
	}

	private void GenerateRegions()
	{
		//throw new NotImplementedException();
	}

	private void RandomizeRegionContent()
	{

	}

    public Vector3 GetPlayerSpawnPoint()
    {
        return new Vector3(spawnRegion.bounds.center.x, spawnRegion.bounds.center.y, 0);
    }

    public Vector3 GetRandomSpawnPoint()
    {
        Region r = regions[Random.Range(2, regions.Count)];

        return new Vector3(r.bounds.center.x, r.bounds.center.y, 0);
    }

    private void CreateAdditionalRegions()
	{
		for (int i = 0; numAddReg < additionalRegionAmount; i++)
		{
			// If a corridor was created, reset counter
			if (CreateRandomRegion())
			{
				i = 0;
				Debug.Log(i + " New region was created. Number of Additional R.: " + numAddReg + " of " + additionalRegionAmount);
			}

			// Cancel generation process of no corridor could be created within x attempts
			if (i >= maxGenerationAttempts)
			{
				Debug.LogWarning(i + " Max iterations reached! Skipping additional region placement.");
				break;
			}
		}
		Debug.Log("Final Number of Additional R.: " + numAddReg + " of " + additionalRegionAmount);
	}

	private void CreateAdditionalCorridors()
	{
		for (int i = 0; numAddCor < additionalCorridorAmount; i++)
        {
			// If a corridor was created, reset counter
			if (CreateRandomCorridor())
			{
				i = 0;
				//Debug.Log(i + " New corridor was created. Number of Additional C.: " + numAddCor + " of " + additionalCorridorAmount);
			}

			// Cancel generation process of no corridor could be created within x attempts
            if (i >= maxGenerationAttempts)
            {
                Debug.LogWarning(i + " Max iterations reached! Skipping additional corridor placement.");
                break;
            }
        }
        Debug.Log("Final Number of Additional C.: " + numAddCor + " of " + additionalCorridorAmount);
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
        List<Region> cors = new List<Region>(corridors);
		bool corridorWasCreated = false;
        while (cors.Count > 0)
        {
            // Get and remove random corridor from list to avoid duplicate checking
            int cIndex = Random.Range(0, cors.Count);
            Region c = cors[cIndex];
            cors.RemoveAt(cIndex);

			// Create a randomly attached region
			if (CreateRandomRegion(c, true))
			{
				corridorWasCreated = true;
				numAddCor++;
				break;
			}
        }

        return corridorWasCreated;
    }

	private bool CreateRandomRegion(Region region = null, bool createCorridor = false)
	{
		// if no region is given, take a random corridor
		if (region == null)
		{
			region = corridors[Random.Range(0, corridors.Count)];
		}

		// Create a list of all possible walls
		List<Region.Wall> walls = new List<Region.Wall>((createCorridor) ? region.GetPerpendicularWalls() : region.walls);
		bool regionWasCreated = false;
		// iterating over all walls in random order
		while (walls.Count > 0)
		{
			// Get and remove random wall from list
			int wIndex = Random.Range(0, walls.Count);
			Region.Wall w = walls[wIndex];
			walls.RemoveAt(wIndex);

			if(PlaceRegionAtRandomSpot(region, w, createCorridor))
			{
				regionWasCreated = true;

				if (!createCorridor) numAddReg++;
				break;
			}
		}

		return regionWasCreated;
	}

	private bool PlaceRegionAtRandomSpot(Region region, Region.Wall wall, bool createCorridor)
	{
		// Test region min width against height or against width of wall
		Vector2Int widthAxis = Region.GetAbsoluteDirectionVector(region.orientation);
		//Debug.Log(numAddCor + " X axis for wall: " + widthAxis.ToString());

		int minWidth = 0, 
			minLength = 0, 
			maxWidth = 0, 
			maxLength = 0;

		if (createCorridor)
		{
			if (widthAxis.x == 0)
			{
				minWidth = corridorLength.min;
				minLength = corridorWidth.min;
				maxWidth = corridorLength.max;
				maxLength = corridorWidth.max;
			}
			else
			{
				minWidth = corridorWidth.min;
				minLength = corridorLength.min;
				maxWidth = corridorWidth.max;
				maxLength = corridorLength.max;
			}
		}
		else
		{
			// TODO
			minWidth = classAreaSize.min;
			minLength = classAreaSize.min;
			maxWidth = classAreaSize.max;
			maxLength = classAreaSize.max;
		}

		// North && South -> Test min region size against horizontal size
		// East && West -> Test min region size against vertical size

		// Test if there is enough space for the smallest possible region
		if (wall.bounds.size.x * widthAxis.x >= minWidth || wall.bounds.size.y * widthAxis.y >= minLength)
		{
			// Add all spots to a list
			List<Vector3Int> spots = new List<Vector3Int>();
			BoundsInt.PositionEnumerator positions = wall.bounds.allPositionsWithin;

			Vector3Int spot;
			// Include corner positions
			Vector3Int minPos = wall.bounds.min - new Vector3Int(widthAxis.x * (Region.cornerThreshold + minWidth), widthAxis.y * (Region.cornerThreshold + minWidth), 0),
				maxPos = wall.bounds.max - new Vector3Int(widthAxis.y * Region.cornerThreshold, widthAxis.x * Region.cornerThreshold, 1);

			//spots.Add(minPos);
			//Debug.Log("min" + minPos);
			// Iterate over all spots and add them to he list
			while (positions.MoveNext())
			{
				spot = positions.Current;
				//Debug.Log(spot);
				if (spot.x <= maxPos.x && spot.y <= maxPos.y && spot.x >= minPos.x && spot.y >= minPos.y)
					spots.Add(spot);
			}
			//spots.Add(maxPos);
			//Debug.Log("max" + maxPos);

			BoundsInt testBounds = new BoundsInt();

			while (spots.Count > 0)
			{
				// Get and remove random spot from list
				int sIndex = Random.Range(0, spots.Count);
				Vector3Int s = spots[sIndex];
				spots.RemoveAt(sIndex);

				// Check if region could be placed at spot
				int pX = 0,
					pY = 0,
					sX = 0,
					sY = 0;

				// Check if every width with any height would overlap another region
				// If not, place region with spot as connection
				bool flip = Region.GetPerpendicularDirectionVector(wall.dir).y == 1;

				for (int l = (flip) ? maxWidth : maxLength; l >= ((flip) ? minWidth : minLength); l--)
				{
					for (int w = (flip) ? maxLength : maxWidth; w >= ((flip) ? minLength: minWidth); w--)
					{
						switch (wall.dir)
						{
							case Region.Direction.NORTH:
								pX = s.x - w + 1;
								pY = s.y;
								sX = w;
								sY = l;
								break;

							case Region.Direction.SOUTH:
								pX = s.x - w + 1;
								pY = s.y - l + 1;
								sX = w;
								sY = l;
								break;

							case Region.Direction.EAST:
								pX = s.x;
								pY = s.y - w + 1;
								sX = l;
								sY = w;
								break;

							case Region.Direction.WEST:
								pX = s.x - l + 1;
								pY = s.y - w + 1;
								sX = l;
								sY = w;
								break;
						}

						testBounds.position = new Vector3Int(pX, pY, 0);
						testBounds.size = new Vector3Int(sX, sY, 1);
						DebugBounds = testBounds;

						bool isOverlapping = false;
						for (int i = 0; i < regions.Count; i++)
						{
							if (Region.BoundsOverlap(regions[i].bounds, testBounds, 1))
							{
								isOverlapping = true;
								break;
							}
						}

						// Place region
						if (!isOverlapping)
						{
							Debug.LogWarning(numAddCor + " Found possible bounds!");
							Range width, length;

							if (flip)
							{
								length = new Range(minWidth, l);
								width = new Range(minLength, w);
							}
							else
							{
								length = new Range(minLength, l);
								width = new Range(minWidth, w);
							}

							// Create region at random spot with given tested parameters
							Region newRegion = MakeRandomRegion(s, wall.dir, width, length);
							newRegion.orientation = Region.GetPerpendicularDirection(region.orientation);

							if (createCorridor)
							{
								newRegion.type = RegionType.Corridor;
							}
							else
							{
								newRegion.type = RegionType.ClassRoom;
							} 

							AddRegion(newRegion, createCorridor);
							ConnectRegions(newRegion, region);
							return true;
						}
					}
				}
			}
		}
		else
		{
			Debug.LogWarning("Wall is not eligable! " + wall.bounds.ToString() + " does not meet " + minWidth + " or " + minLength);
		}
		return false;
	}

	private Region MakeRandomRegion(Vector3Int spot, Region.Direction dir, Range width, Range length)
	{
		int sX = Random.Range(width.min, width.max + 1);
		int sY = Random.Range(length.min, length.max + 1);

		//Debug.Log("Creating region with width of " + sX + " " + width.ToString() + " & length of " + sY + " " + length.ToString());

		int pX = 0,
			pY = 0;

		switch (dir)
		{
			case Region.Direction.NORTH:
				pX = spot.x - sX + 1;
				pY = spot.y;
				break;

			case Region.Direction.SOUTH:
				pX = spot.x - sX + 1;
				pY = spot.y - sY + 1;
				break;

			case Region.Direction.EAST:
				sX = sX + sY;
				sY = sX - sY;
				sX = sX - sY;
				pX = spot.x;
				pY = spot.y - sY + 1;
				break;

			case Region.Direction.WEST:
				sX = sX + sY;
				sY = sX - sY;
				sX = sX - sY;
				pX = spot.x - sX + 1;
				pY = spot.y - sY + 1;
				break;
		}

		BoundsInt newBounds = new BoundsInt(pX, pY, 0, sX, sY, 1);
		newBounds.ClampToBounds(generationBounds);
		DebugBounds = newBounds;
		return new Region(newBounds, RegionType.None);
	}

	private void AddRegion(Region region, bool isCorrdior = false)
	{
		int translatedX = region.bounds.x - generationBounds.x,
			translatedY = region.bounds.y - generationBounds.y;
		//Debug.Log(region.bounds.ToString() + " -> " + translatedX + ", " + translatedY);

		// Add to region map
		for (int x = translatedX; x < translatedX + region.bounds.size.x; x++)
		{
			for (int y = translatedY; y < translatedY + region.bounds.size.y; y++)
			{
				regionMap[x, y] = new RegionSpot(region.type, region.Id);
			}
		}

		// Add to map
		for (int x = translatedX; x < translatedX + region.bounds.size.x; x++)
		{
			for (int y = translatedY; y < translatedY + region.bounds.size.y; y++)
			{
				TileType type = TileType.Ground;
				if (x == translatedX || y == translatedY || x == translatedX + region.bounds.size.x - 1 || y == translatedY + region.bounds.size.y - 1)
				{
					type = TileType.Wall;
				}
				map[x, y] = type;
			}
		}

		regions.Add(region);
		if (isCorrdior) corridors.Add(region);
	}

	private void ConnectRegions(Region a, Region b)
	{
		BoundsInt overlap = a.ConnectToRegion(b);
		int translatedX = overlap.x - generationBounds.x,
			translatedY = overlap.y - generationBounds.y;

		// Update map
		for (int x = translatedX; x < translatedX + overlap.size.x; x++)
		{
			for (int y = translatedY; y < translatedY + overlap.size.y; y++)
			{
				map[x, y] = TileType.Doorway;
			}
		}
	}

	private void SetupSpawn()
	{
		// Find Spawn Region or create a new one
		spawnRegion = regions.Find(x => x.isSpawn);
		if (spawnRegion == null)
		{
			Vector3Int size = new Vector3Int(spawnAreaWidth, spawnAreaHeight, 1);
			Vector3Int position = Vector3Int.RoundToInt(generationBounds.center - new Vector3(size.x / 2f, size.y / 2f));
			spawnRegion = new Region(new BoundsInt(position, size), RegionType.Toilet);
			spawnRegion.isSpawn = true;

			AddRegion(spawnRegion);
		}

		// Find Main Corridor or create a new one
		mainCorridor = regions.Find(x => x.type == RegionType.MainCorridor);
		if (mainCorridor == null)
		{
			Vector3Int size = new Vector3Int(mainCorridorWidth, mainCorridorHeight, 1);
			Region.Wall w = spawnRegion.walls.Find(x => x.dir == Region.Direction.EAST);
			Vector3Int position = Vector3Int.RoundToInt(w.bounds.center - Vector3.Scale(size, new Vector3(0, .5f)));
			mainCorridor = new Region(new BoundsInt(position, size), RegionType.MainCorridor);

			AddRegion(mainCorridor);
			corridors.Add(mainCorridor);
		}

		// Connect rooms
		if (!spawnRegion.isConnected)
		{
			ConnectRegions(spawnRegion, mainCorridor);
		}
	}

	private void CreateTileMap()
    {
        for (int x = 0; x < generationBounds.size.x; x++)
        {
            for (int y = 0; y < generationBounds.size.y; y++)
            {
				if (map[x, y] == TileType.Air) continue;

                Vector3 position = new Vector3(generationBounds.xMin + (x + 0.5f) * tileSize,
                    generationBounds.yMin + (y + 0.5f * tileSize) * tileSize);
                Vector3 size = Vector3.Scale(Vector3.one, new Vector3(tileSize, tileSize, 1f));

                GameObject tilePrefab; 
                TileType tileType = map[x, y];
                tilePrefab = tileSprites[(int)tileType].tilePrefabs[0];

                tilemap[x, y] = Instantiate(tilePrefab, position, Quaternion.identity, parent.transform);
				tilemap[x, y].transform.localScale = size;

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

        map = new TileType[generationBounds.size.x, generationBounds.size.y];
		regionMap = new RegionSpot[generationBounds.size.x, generationBounds.size.y];
        tilemap = new GameObject[generationBounds.size.x, generationBounds.size.y];

        for (int x = 0; x < generationBounds.size.x; x++)
        {
            for (int y = 0; y < generationBounds.size.y; y++)
            {
                map[x, y] = TileType.Air;
                regionMap[x, y] = new RegionSpot(RegionType.None, -1);
				tilemap[x, y] = null;
            }
        }

		numAddReg = 0;
		numAddCor = 0;
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
                        TileType tile = map[x, y];
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
					foreach (Region.Wall w in r.walls)
					{
						Gizmos.color = Color.red;

						Gizmos.DrawLine(new Vector3(w.bounds.xMin, w.bounds.yMin), new Vector3(w.bounds.xMin, w.bounds.yMax));
						Gizmos.DrawLine(new Vector3(w.bounds.xMin, w.bounds.yMin), new Vector3(w.bounds.xMax, w.bounds.yMin));
						Gizmos.DrawLine(new Vector3(w.bounds.xMax, w.bounds.yMax), new Vector3(w.bounds.xMin, w.bounds.yMax));
						Gizmos.DrawLine(new Vector3(w.bounds.xMax, w.bounds.yMax), new Vector3(w.bounds.xMax, w.bounds.yMin));
					}
					foreach (BoundsInt b in r.connections)
					{
						Gizmos.color = Color.magenta;

						Gizmos.DrawLine(new Vector3(b.xMin, b.yMin), new Vector3(b.xMin, b.yMax));
						Gizmos.DrawLine(new Vector3(b.xMin, b.yMin), new Vector3(b.xMax, b.yMin));
						Gizmos.DrawLine(new Vector3(b.xMax, b.yMax), new Vector3(b.xMin, b.yMax));
						Gizmos.DrawLine(new Vector3(b.xMax, b.yMax), new Vector3(b.xMax, b.yMin));
					}
					Gizmos.color = Color.yellow;

					Gizmos.DrawLine(new Vector3(r.bounds.xMin, r.bounds.yMin), new Vector3(r.bounds.xMin, r.bounds.yMax));
					Gizmos.DrawLine(new Vector3(r.bounds.xMin, r.bounds.yMin), new Vector3(r.bounds.xMax, r.bounds.yMin));
					Gizmos.DrawLine(new Vector3(r.bounds.xMax, r.bounds.yMax), new Vector3(r.bounds.xMin, r.bounds.yMax));
					Gizmos.DrawLine(new Vector3(r.bounds.xMax, r.bounds.yMax), new Vector3(r.bounds.xMax, r.bounds.yMin));
				}
			}
        }

        if (DebugBounds != null)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawLine(new Vector3(DebugBounds.xMin, DebugBounds.yMin), new Vector3(DebugBounds.xMin, DebugBounds.yMax));
            Gizmos.DrawLine(new Vector3(DebugBounds.xMin, DebugBounds.yMin), new Vector3(DebugBounds.xMax, DebugBounds.yMin));
            Gizmos.DrawLine(new Vector3(DebugBounds.xMax, DebugBounds.yMax), new Vector3(DebugBounds.xMin, DebugBounds.yMax));
            Gizmos.DrawLine(new Vector3(DebugBounds.xMax, DebugBounds.yMax), new Vector3(DebugBounds.xMax, DebugBounds.yMin));
        }
    }
}
