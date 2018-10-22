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
	public Tileset floorTiles;
	public Tileset wallTiles;
	public Tileset connectionTiles;
	#endregion

	#region Private Variables
    private List<Region> regions;
	private RegionSpot[,] regionMap;

	private List<Region> rooms;
	private List<Region> corridors;

	//private TileType[,] map;
    private Furniture[,] obstacleMap;
    private GameObject[,] tileMap;

	private GameObject parent;

    Region spawnRoom, mainCorridor;
    int numAddCor, numAddRoom;

    BoundsInt DebugBounds = new BoundsInt();
    #endregion

    public void GenerateLayout()
	{
		InitializeRandom();
		InitializeMap();

        SetupSpawn();

        CreateAdditionalCorridors();
        GenerateRegions();
		CreateAdditionalRooms();
		RandomizeRegionContent();

        CreateTileMap();
	}

	private void GenerateRegions()
	{
		//throw new NotImplementedException();

		// Add a principals office
		// Add a gym
		// Add a music room
		// Add a chemistry room

	}

	private void RandomizeRegionContent()
	{

	}

    public Vector3 GetPlayerSpawnPoint()
    {
        return new Vector3(spawnRoom.bounds.center.x, spawnRoom.bounds.center.y, 0);
    }

    public Vector3 GetRandomSpawnPoint(bool excludeCorridors = false)
    {
		Region r;
		if (excludeCorridors)
		{
			if (rooms.Count > 1)
			{
				r = rooms[Random.Range(1, rooms.Count)];
			} else
			{
				r = mainCorridor;
			}
		}
		else
		{
			r = regions[Random.Range(2, regions.Count)];
		}

        return new Vector3(r.bounds.center.x, r.bounds.center.y, 0);
    }

    private void CreateAdditionalRooms()
	{
		for (int i = 0; numAddRoom < additionalRegionAmount; i++)
		{
			// If a corridor was created, reset counter
			if (CreateRandomRoom())
			{
				i = 0;
				//Debug.Log(i + " New region was created. Number of Additional R.: " + numAddReg + " of " + additionalRegionAmount);
			}

			// Cancel generation process of no corridor could be created within x attempts
			if (i >= maxGenerationAttempts)
			{
				Debug.LogWarning(i + " Max iterations reached! Skipping additional region placement.");
				break;
			}
		}
		Debug.Log("Final Number of Additional R.: " + numAddRoom + " of " + additionalRegionAmount);
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
        List<Region> cors = new List<Region>(corridors);
		bool corridorWasCreated = false;
        while (cors.Count > 0)
        {
            // Get and remove random corridor from list to avoid duplicate checking
            int cIndex = Random.Range(0, cors.Count);
            Region c = cors[cIndex];
            cors.RemoveAt(cIndex);

			// Create a randomly attached region
			if (CreateRandomRoom(c, true))
			{
				corridorWasCreated = true;
				numAddCor++;
				break;
			}
        }

        return corridorWasCreated;
    }

	private bool CreateRandomRoom(Region region = null, bool createCorridor = false)
	{
		// if no region is given, take a random corridor for connection
		if (region == null)
		{
			region = corridors[Random.Range(0, corridors.Count)];
		}

		// Create a list of all possible walls
		List<Region.Wall> walls = new List<Region.Wall>((createCorridor) ? region.GetPerpendicularWalls() : region.walls);
		bool roomWasCreated = false;
		// iterating over all walls in random order
		while (walls.Count > 0)
		{
			// Get and remove random wall from list
			int wIndex = Random.Range(0, walls.Count);
			Region.Wall w = walls[wIndex];
			walls.RemoveAt(wIndex);

			Region newRoom = GenerateRegionAtRandomSpot(w, createCorridor);

			if (newRoom != null)
			{
				int connectionSize = -1;
				if (!createCorridor)
				{
					numAddRoom++;
					connectionSize = 2;
				}

				roomWasCreated = true;
				AddRegion(newRoom, createCorridor);
				ConnectRegions(region, newRoom, connectionSize);

				
				break;
			}
		}

		return roomWasCreated;
	}

	// Returns the new region
	private Region GenerateRegionAtRandomSpot(Region.Wall wall, bool createCorridor)
	{
		Vector2Int widthAxis = Region.GetPerpendicularDirectionVector(wall.dir);

		int minWidth = 0, 
			minLength = 0, 
			maxWidth = 0, 
			maxLength = 0;

		// Determine RegionType and it's generation parameters
		// For now there are only corridors and classrooms
		//RegionType newRegionType = (RegionType) Random.Range((int)RegionType.ClassRoom, (int)RegionType.Storage);
		RegionType newRegionType;

		if (createCorridor)
		{
			newRegionType = RegionType.Corridor;
		}
		else
		{
			newRegionType = RegionType.ClassRoom;
		}

		if (newRegionType == RegionType.Corridor)
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
			// Width axis doesn't matter for class rooms
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

			// Determine first and last possible random spot
			Vector3Int width3Axis  = new Vector3Int(widthAxis.x, widthAxis.y, 0);
			Vector3Int length3Axis = new Vector3Int(1 - widthAxis.x, 1 - widthAxis.y, 0);

			Vector3Int minPos = wall.bounds.min + new Vector3Int(Region.cornerThreshold, Region.cornerThreshold, 0) * width3Axis;
			Vector3Int maxPos = wall.bounds.max	- new Vector3Int(Region.cornerThreshold, Region.cornerThreshold, 0) * width3Axis - new Vector3Int(length3Axis.x, length3Axis.y, 1);

			Vector3Int minSpot = Vector3Int.Scale(minPos, width3Axis);
			Vector3Int maxSpot = Vector3Int.Scale(maxPos, width3Axis);

			int counter = minSpot.x + minSpot.y;
			int maxCounter = maxSpot.x + maxSpot.y;

			if (counter > maxCounter)
			{
				maxCounter += counter;
				counter = maxCounter - counter;
				maxCounter = maxCounter - counter;
			}

			/*Debug.Log("Wall dir [" + wall.dir + "] Bounds: " + wall.bounds + " (min: " + wall.bounds.min + " | max: " + wall.bounds.max + ")");
			Debug.Log("Spots: " + minSpot + " | " + maxSpot + " | " + minPos + " | " + maxPos);
			Debug.Log("Counting: " + counter + " | " + maxCounter + " | " + widthAxis);
			*/
			int stepSize = 2;

			Vector3Int basePos = wall.bounds.min * (Vector3Int.one - width3Axis);

			for (; counter <= maxCounter; counter += stepSize)
			{
				Vector3Int point = basePos;
				point.x += widthAxis.x * counter;
				point.y += widthAxis.y * counter;

				spots.Add(point);
			}
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
								pY = s.y - 1;
								sX = w;
								sY = l;
								break;

							case Region.Direction.SOUTH:
								pX = s.x - w + 1;
								pY = s.y - l;
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
							Debug.Log("Spot: " + s.ToString() + ", Dir: " + wall.dir);
							Debug.Log("Test Region " + new Region(testBounds, RegionType.None, Region.GetOppositeDirection(wall.dir)).ToString());

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

							newRegion.type = newRegionType;

							Debug.Log("New Region " + newRegion.ToString());

							return newRegion;
						}
					}
				}
			}
		}
		else
		{
			//Debug.LogWarning("Wall is not eligable! " + wall.bounds.ToString() + " does not meet " + minWidth + " or " + minLength);
		}
		return null;
	}

	private Region MakeRandomRegion(Vector3Int spot, Region.Direction dir, Range width, Range length)
	{
		int sX = Random.Range(width.min, width.max + 1);
		int sY = Random.Range(length.min, length.max + 1);

		Debug.Log("Creating region with width of " + sX + " " + width.ToString() + " & length of " + sY + " " + length.ToString());

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
		DebugBounds = newBounds;

		return new Region(newBounds, RegionType.None, Region.GetOppositeDirection(dir));
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

		regions.Add(region);
		if (isCorrdior) corridors.Add(region);
		else rooms.Add(region);

		/*// Add to map
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
		}*/


	}

	private void ConnectRegions(Region a, Region b, int connectionSize)
	{
		BoundsInt overlap = a.ConnectToRegion(b, connectionSize);

		/*int translatedX = overlap.x - generationBounds.x,
			translatedY = overlap.y - generationBounds.y;

		// Update map
		for (int x = translatedX; x < translatedX + overlap.size.x; x++)
		{
			for (int y = translatedY; y < translatedY + overlap.size.y; y++)
			{
				map[x, y] = TileType.Doorway;
			}
		}*/
	}

	private void SetupSpawn()
	{
		// Find Spawn Region or create a new one
		spawnRoom = regions.Find(x => x.isSpawn);
		if (spawnRoom == null)
		{
			Vector3Int size = new Vector3Int(spawnAreaWidth, spawnAreaHeight, 1);
			Vector3Int position = Vector3Int.RoundToInt(generationBounds.center - new Vector3(size.x / 2f, size.y / 2f));
			spawnRoom = new Region(new BoundsInt(position, size), RegionType.Toilet);
			spawnRoom.isSpawn = true;

			AddRegion(spawnRoom);
		}

		// Find Main Corridor or create a new one
		mainCorridor = regions.Find(x => x.type == RegionType.MainCorridor);
		if (mainCorridor == null)
		{
			Vector3Int size = new Vector3Int(mainCorridorWidth, mainCorridorHeight, 1);
			Region.Wall w = spawnRoom.walls.Find(x => x.dir == Region.Direction.EAST);
			Vector3Int position = Vector3Int.RoundToInt(w.bounds.center - Vector3.Scale(size, new Vector3(0, .5f)));
			mainCorridor = new Region(new BoundsInt(position, size), RegionType.MainCorridor);

			AddRegion(mainCorridor, true);
		}

		// Connect rooms
		if (!spawnRoom.isConnected)
		{
			ConnectRegions(spawnRoom, mainCorridor, 1);
		}
	}

	private void CreateTileMap()
    {
		// Iterate over each region and instantiate it's tiles
		for (int i = 0; i < regions.Count; i++)
		{
			Region r = regions[i];
			Vector3Int basePos = r.bounds.min;
			for (int x = 0; x < r.bounds.size.x; x++)
			{
				for (int y = 0; y < r.bounds.size.y; y++)
				{
					GameObject tilePrefab;

					// Walls
					if (x == 0 || y == 0 || x == r.bounds.size.x - 1 || y == r.bounds.size.y - 1)
					{
						tilePrefab = wallTiles.Middle;
					}
					else if (x == 1 && y == 1) // bottom left ground
					{
						tilePrefab = floorTiles.BottomLeft;
					}
					else if (x == r.bounds.size.x - 2 && y == 1) // bottom right ground
					{
						tilePrefab = floorTiles.BottomRight;
					}
					else if (x == 1 && y == r.bounds.size.y - 2) // top left ground
					{
						tilePrefab = floorTiles.TopLeft;
					}
					else if (x == r.bounds.size.x - 2 && y == r.bounds.size.y - 2) // bottom right ground
					{
						tilePrefab = floorTiles.TopRight;
					}
					else if (y == 1) // bottom ground
					{
						tilePrefab = floorTiles.Bottom;
					}
					else if (x == 1) // left ground
					{
						tilePrefab = floorTiles.Left;
					}
					else if (x == r.bounds.size.x - 2) // right ground
					{
						tilePrefab = floorTiles.Right;
					}
					else if (y == r.bounds.size.y - 2) // top ground
					{
						tilePrefab = floorTiles.Top;
					}
					else // center ground
					{
						tilePrefab = floorTiles.Middle;
					}

					SpawnTile(basePos, new Vector2Int(x, y), tilePrefab);
				}
			}
			for (int j = 0; j < r.connections.Count; j++)
			{
				BoundsInt con = r.connections[j];
				Vector3Int conBasePos = con.position;
				for (int x = 0; x < con.size.x; x++)
				{
					for (int y = 0; y < con.size.y; y++)
					{
						GameObject tilePrefab = connectionTiles.Middle;
						SpawnTile(conBasePos, new Vector2Int(x, y), tilePrefab);	
					}
				}
			}
		}

        /*for (int x = 0; x < generationBounds.size.x; x++)
        {
            for (int y = 0; y < generationBounds.size.y; y++)
            {
				if (map[x, y] == TileType.Air) continue;

                Vector3 position = new Vector3(generationBounds.xMin + (x + 0.5f) * tileSize,
                    generationBounds.yMin + (y + 0.5f * tileSize) * tileSize);


                GameObject tilePrefab; 
                TileType tileType = map[x, y];
                tilePrefab = tileSprites[(int)tileType].tilePrefabs[0];

                tilemap[x, y] = Instantiate(tilePrefab, position, Quaternion.identity, parent.transform);
				tilemap[x, y].transform.localScale = size;

            }
        }*/
    }

	private void SpawnTile (Vector3 basePosition, Vector2Int relativePosition, GameObject tilePrefab)
	{
		// Tile Size
		Vector3 size = Vector3.Scale(Vector3.one, new Vector3(tileSize, tileSize, 1f));
		Vector2Int mapPos = GetPositionInMap(basePosition) + relativePosition;
		Vector3 pos = new Vector3(generationBounds.xMin + (mapPos.x + 0.5f) * tileSize,
			generationBounds.yMin + (mapPos.y + 0.5f) * tileSize);

		if (tileMap[mapPos.x, mapPos.y] != null && tileMap[mapPos.x, mapPos.y] != connectionTiles.Middle)
		{
			Destroy(tileMap[mapPos.x, mapPos.y]);
		}

		tileMap[mapPos.x, mapPos.y] = Instantiate(tilePrefab, pos, Quaternion.identity, parent.transform);
		tileMap[mapPos.x, mapPos.y].transform.localScale = size;
	}

	private Vector2Int GetPositionInMap(Vector3 absolutePosition)
	{
		int offsetX = generationBounds.size.x + generationBounds.xMin;
		int offsetY = generationBounds.size.y + generationBounds.yMin;

		int x = Mathf.RoundToInt(absolutePosition.x + offsetX);
		int y = Mathf.RoundToInt(absolutePosition.y + offsetY);

		return new Vector2Int(x, y);
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
        rooms = new List<Region>();

		//map = new TileType[generationBounds.size.x, generationBounds.size.y];
		regionMap = new RegionSpot[generationBounds.size.x, generationBounds.size.y];
        tileMap = new GameObject[generationBounds.size.x, generationBounds.size.y];

        for (int x = 0; x < generationBounds.size.x; x++)
        {
            for (int y = 0; y < generationBounds.size.y; y++)
            {
        //        map[x, y] = TileType.Air;
                regionMap[x, y] = new RegionSpot(RegionType.None, -1);
				tileMap[x, y] = null;
            }
        }

		numAddRoom = 0;
		numAddCor = 0;
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
				}
			}
        }

        if (DebugBounds != null)
        {
            Gizmos.color = Color.green;

            Gizmos.DrawLine(new Vector3(DebugBounds.xMin, DebugBounds.yMin), new Vector3(DebugBounds.xMin, DebugBounds.yMax));
            Gizmos.DrawLine(new Vector3(DebugBounds.xMin, DebugBounds.yMin), new Vector3(DebugBounds.xMax, DebugBounds.yMin));
            Gizmos.DrawLine(new Vector3(DebugBounds.xMax, DebugBounds.yMax), new Vector3(DebugBounds.xMin, DebugBounds.yMax));
            Gizmos.DrawLine(new Vector3(DebugBounds.xMax, DebugBounds.yMax), new Vector3(DebugBounds.xMax, DebugBounds.yMin));
        }
    }
}
