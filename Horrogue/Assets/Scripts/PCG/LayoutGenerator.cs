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
	[Header("General", order = 0)]
	public bool useRandomSeed = false;	// Should a seed be generated
	public string seed = "elementary";  // The current seed used for generation

	// The bounds where and how big the region generation is going to be
	public BoundsInt generationBounds = new BoundsInt(-50, -50, 0, 100, 100, 1);

    // How many times the generator should try placing content before skipping to the next step
    public int maxGenerationAttempts = 25;

	[Header("New Spawning Behaviour", order = 0)]
	[Header("Amounts", order = 1)]
	public int additionalCorridorAmount = 2;
	public int additionalRoomAmount = 10;

	[Header("Prefabs", order = 1)]
	public PremadeRegion spawnRoomLayout;
	public PremadeRegion exitRoomLayout;
	public PremadeRegion mainCorridorLayout;

	public List<PremadeRegion> otherPremadeLayouts;

	public CustomRegion corridorLayouts;
	public CustomRegion roomLayouts;

	

	// Premade objects and regions
	[Header("Spawning Behaviour")]
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

    private GameObject[,] tileMap;

	private GameObject tileParent;
	private GameObject furnitureParent;

    Region spawnRoom, mainCorridor, exitRoom;
	Direction lastRandConDir;
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

        CreateTileMap();
		PlaceRegionContents();
	}

	public GameObject GetExit()
	{
		return exitRoom.placedFurnitures[0];
	}

	private void GenerateRegions()
	{

		// Create the exit and flag it
		CreateRandomRoom(null, false, exitRoomLayout);
		exitRoom = regions.Find(x => x.type == RegionType.Exit);
		exitRoom.isExit = true;

		for (int i = 0; i < otherPremadeLayouts.Count; i++)
		{
			PremadeRegion layout = otherPremadeLayouts[i];
			CreateRandomRoom(null, false, layout);
		}
	}

	private void PlaceRegionContents()
	{
		for (int i = 0; i < regions.Count; i++)
		{
			Region r = regions[i];
			if (!r.isFurnished)
			{
				for (int j = 0; j < r.furnitures.Count; j++)
				{
					RegionFurnitures rf = r.furnitures[j];
					PlaceRegionFurnitures(r, rf);
				}

				for (int j = 0; j < r.variableFurnitures.Count; j++)
				{
					VariableRegionFurnitures vrf = r.variableFurnitures[j];
					PlaceRegionFurnitures(r, (RegionFurnitures)vrf, vrf.spawnAmount);
				}

				regions[i].isFurnished = true;
			}
		}
	}

	private void PlaceRegionFurnitures(Region r, RegionFurnitures furnitures, int amount = -1)
	{
		/*if (furnitures.prefab == null || furnitures.spawnTransforms == null || furnitures.spawnTransforms.Count == 0)
			throw new Exception("Place Region Furnitures - " + "No furniture prefab or spawn transforms have been assigned! " + furnitures.ToString());
			*/
		GameObject prefab = furnitures.prefab;
		if (prefab == null)
		{
			prefab = new GameObject(furnitures.name);
		}
		Vector3 basePos = r.innerBounds.center;
		List<LocalTransform> localTransforms = new List<LocalTransform>(furnitures.spawnTransforms);

		while (localTransforms.Count > 0 && (amount > 0 || amount == -1))
		{
			LocalTransform lt = localTransforms[Random.Range(0, localTransforms.Count)];
			localTransforms.Remove(lt);
			GameObject placedFurniture = Instantiate(prefab, basePos + lt.position, lt.rotation, furnitureParent.transform);
			placedFurniture.transform.localScale = lt.scale;
			r.placedFurnitures.Add(placedFurniture);
		}
	}

    public Vector3 GetPlayerSpawnPoint()
    {
        return new Vector3(spawnRoom.outerBounds.center.x, spawnRoom.outerBounds.center.y, 0);
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
		if (r == exitRoom)
		{
			Debug.Log("Removing exit room from list");
			regions.Remove(exitRoom);
			return GetRandomSpawnPoint(excludeCorridors);
		}
        return new Vector3(r.outerBounds.center.x, r.outerBounds.center.y, 0);
    }

    private void CreateAdditionalRooms()
	{
		for (int i = 0; numAddRoom < additionalRoomAmount; i++)
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
		Debug.Log("Final Number of Additional R.: " + numAddRoom + " of " + additionalRoomAmount);
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
            Region c = cors[Random.Range(0, cors.Count)];
            cors.Remove(c);

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

	private bool CreateRandomRoom(Region region = null, bool createCorridor = false, PremadeRegion regionLayout = null)
	{
		// if no region is given, take a random corridor for connection
		List<Region> connectableRegions;
		if (region == null)
		{
			connectableRegions = new List<Region>(corridors);
		}
		else
		{
			connectableRegions = new List<Region>() { region };
		}

		bool roomWasCreated = false;
		while (connectableRegions.Count > 0 && !roomWasCreated)
		{
			// Get and remove random corridor from list to avoid duplicate checking
			Region cr = connectableRegions[Random.Range(0, connectableRegions.Count)];
			connectableRegions.Remove(cr);

			// Create a list of all possible walls
			List<Wall> walls = new List<Wall>((createCorridor) ? cr.GetPerpendicularWalls() : cr.walls);

			// iterating over all walls in random order
			while (walls.Count > 0)
			{
				// Get and remove random wall from list
				Wall w = walls[Random.Range(0, walls.Count)];
				walls.Remove(w);

				Region newRoom = CreateRegionAtRandomConnection(w, createCorridor, regionLayout); //GenerateRegionAtRandomSpot(w, createCorridor);

				if (newRoom != null)
				{
					if (!createCorridor && regionLayout == null)
					{
						numAddRoom++;
					}
					newRoom.orientation = Region.GetPerpendicularDirection(cr.orientation);
					roomWasCreated = true;
					AddRegion(newRoom, createCorridor);
					ConnectRegions(cr, newRoom);

					//Debug.Log(newRoom.orientation);
					break;
				}
			}
		}
		return roomWasCreated;
	}

	private Region CreateRegionAtRandomConnection(Wall w, bool createCorridor, PremadeRegion regionLayout = null)
	{
		CustomRegion customRegion;
		List<VariantRegion> possibleRegionLayouts;
		List<BoundsInt> possibleConnections = new List<BoundsInt>(w.possibleConnections);
		if (regionLayout == null)
		{
			if (createCorridor)
			{
				customRegion = corridorLayouts;
			}
			else
			{
				customRegion = roomLayouts;
			}
			//Debug.Log("Create Region At Random Connection - " + "useSeperateVerticalRegions: " + customRegion.useSeperateVerticalRegions + " | Wall is Vertical: " + w.isVertical);
			if (customRegion.useSeperateVerticalRegions && !w.isVertical)
			{
				possibleRegionLayouts = customRegion.verticalRegionVariations;
			}
			else
			{
				possibleRegionLayouts = customRegion.regionVariations;
			}
		}
		else
		{
			//Debug.Log("Creating a premade region.");
			customRegion = new CustomRegion();



			VariantRegion vr;
			if (regionLayout.GetType() == typeof(VariantRegion))
			{
				vr = (VariantRegion) regionLayout;
			}
			else
			{
				vr = new VariantRegion(regionLayout);
			}
			possibleRegionLayouts = new List<VariantRegion>() { vr };
			customRegion.regionVariations = possibleRegionLayouts;
			customRegion.type = regionLayout.type;
		}
		while (possibleConnections.Count > 0)
		{
			// Get and remove a connection from the list
			BoundsInt con = possibleConnections[Random.Range(0, possibleConnections.Count)];
			possibleConnections.Remove(con);

			List<VariantRegion> regionLayouts = new List<VariantRegion>(possibleRegionLayouts);
			
			while(regionLayouts.Count > 0)
			{
				// Get and remove a layout from the list to avoid duplicate checking
				VariantRegion layout = regionLayouts[Random.Range(0, regionLayouts.Count)];
				regionLayouts.Remove(layout);

				// Test layout connections against possbile connections
				RegionConnections vrcs = layout.connections.Find(x => x.direction == Region.GetOppositeDirection(w.dir));
				if (vrcs == null) continue;

				List <BoundsInt> variantConnections = new List<BoundsInt>(vrcs.boundsList);
				while (variantConnections.Count > 0)
				{
					// Get and remove a connection from the list
					BoundsInt varcon = variantConnections[Random.Range(0, variantConnections.Count)];
					variantConnections.Remove(varcon);

					Vector3Int alignmentOffset = AlignConnections(con, varcon, w.dir);

					Vector3Int size = new Vector3Int(layout.innerRegionWidth, layout.innerRegionLength, 1);
					Vector3Int position = Vector3Int.RoundToInt(alignmentOffset - new Vector3(size.x / 2f, size.y / 2f));

					BoundsInt testBounds = DebugBounds = Region.GetOuterBounds(position, size);
					

					bool isOverlapping = false;
					for (int i = 0; i < regions.Count; i++)
					{
						if (Region.BoundsOverlap(regions[i].outerBounds, testBounds, 1))
						{
							isOverlapping = true;
							break;
						}
					}

					// Place region
					if (!isOverlapping)
					{
						Region newRegion = new Region(layout, position);

						if (newRegion.type == RegionType.None)
						{
							newRegion.type = customRegion.type;
						}

						if (newRegion.floorTiles == null)
						{
							newRegion.floorTiles = customRegion.tilesets[Random.Range(0, customRegion.tilesets.Count)];
						}
						return newRegion;
					}
				}
			}
		}

		// No new Region could be created
		return null;
	}

	// Returns the new region
	private Region GenerateRegionAtRandomSpot(Wall wall, bool createCorridor)
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
							case Direction.NORTH:
								pX = s.x - w + 1;
								pY = s.y - 1;
								sX = w;
								sY = l;
								break;

							case Direction.SOUTH:
								pX = s.x - w + 1;
								pY = s.y - l;
								sX = w;
								sY = l;
								break;

							case Direction.EAST:
								pX = s.x;
								pY = s.y - w + 1;
								sX = l;
								sY = w;
								break;

							case Direction.WEST:
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
							if (Region.BoundsOverlap(regions[i].outerBounds, testBounds, 1))
							{
								isOverlapping = true;
								break;
							}
						}

						// Place region
						if (!isOverlapping)
						{
							//Debug.Log("Spot: " + s.ToString() + ", Dir: " + wall.dir);
							//Debug.Log("Test Region " + new Region(testBounds, RegionType.None, Region.GetOppositeDirection(wall.dir)).ToString());

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

							//Debug.Log("New Region " + newRegion.ToString());

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

	private Region MakeRandomRegion(Vector3Int spot, Direction dir, Range width, Range length)
	{
		int sX = Random.Range(width.min, width.max + 1);
		int sY = Random.Range(length.min, length.max + 1);

		//Debug.Log("Creating region with width of " + sX + " " + width.ToString() + " & length of " + sY + " " + length.ToString());

		int pX = 0,
			pY = 0;

		switch (dir)
		{
			case Direction.NORTH:
				pX = spot.x - sX + 1;
				pY = spot.y;
				break;

			case Direction.SOUTH:
				pX = spot.x - sX + 1;
				pY = spot.y - sY + 1;
				break;

			case Direction.EAST:
				sX = sX + sY;
				sY = sX - sY;
				sX = sX - sY;
				pX = spot.x;
				pY = spot.y - sY + 1;
				break;

			case Direction.WEST:
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
		int translatedX = region.outerBounds.x - generationBounds.x,
			translatedY = region.outerBounds.y - generationBounds.y;
		//Debug.Log(region.bounds.ToString() + " -> " + translatedX + ", " + translatedY);

		// Add to region map
		for (int x = translatedX; x < translatedX + region.outerBounds.size.x; x++)
		{
			for (int y = translatedY; y < translatedY + region.outerBounds.size.y; y++)
			{
				regionMap[x, y] = new RegionSpot(region.type, region.Id);
			}
		}

		regions.Add(region);
		if (isCorrdior) corridors.Add(region);
		else rooms.Add(region);
	}

	private void ConnectRegions(Region a, Region b)
	{
		a.ConnectToRegion(b);
	}

	private void ConnectRegions(Region a, Region b, int connectionSize)
	{
		BoundsInt overlap = a.ConnectToRegion(b, connectionSize);
	}

	private void SetupSpawn()
	{
		// Create Spawn Region
		Vector3Int spawnSize = new Vector3Int(spawnRoomLayout.innerRegionWidth, spawnRoomLayout.innerRegionLength, 1);
		Vector3Int spawnPosition = Vector3Int.RoundToInt(generationBounds.center - new Vector3(spawnSize.x / 2f, spawnSize.y / 2f));
		spawnRoom = new Region(spawnRoomLayout, spawnPosition);
		spawnRoom.isSpawn = true;

		AddRegion(spawnRoom);

		// Create Main Corridor
		// Pick random connection from spawn and calcualte connection offset
		BoundsInt connection = GetRandomConnection(spawnRoom);
		BoundsInt variantConnection = GetRandomConnection((VariantRegion)mainCorridorLayout, Region.GetOppositeDirection(lastRandConDir));
		Vector3Int alignmentOffset = AlignConnections(connection, variantConnection, lastRandConDir);

		// Calculate the corrdiors position
		Vector3Int mainCorridorSize = new Vector3Int(mainCorridorLayout.innerRegionWidth, mainCorridorLayout.innerRegionLength, 1);
		Vector3Int mainCorridorPosition = Vector3Int.RoundToInt(alignmentOffset - new Vector3(mainCorridorSize.x / 2f, mainCorridorSize.y / 2f));
		mainCorridor = new Region((VariantRegion)mainCorridorLayout, mainCorridorPosition);

		mainCorridor.type = RegionType.MainCorridor;
		mainCorridor.orientation = Direction.NORTH;

		AddRegion(mainCorridor, true);

		ConnectRegions(spawnRoom, mainCorridor);
	}

	private Vector3Int AlignConnections(BoundsInt connection, BoundsInt otherConnection, Direction connectionSide)
	{
		// B to A = A - B, add Wall Thickness too?
		Vector3Int offset = Vector3Int.FloorToInt(connection.center - otherConnection.center) - Wall.GetDirectionThickness(Region.GetOppositeDirection(connectionSide));
		if (connectionSide == Direction.SOUTH || connectionSide == Direction.WEST)
		{
			offset += Region.GetDirectionVector(Region.GetOppositeDirection(connectionSide));
		}
		//Debug.Log("Align Connections - " + "Connection: " + connection.ToString() + ", Other Connection: " + otherConnection.ToString());
		return offset;
	}

	private BoundsInt GetRandomConnection(Region region)
	{
		List<Wall> walls = region.walls.FindAll(x => x.possibleConnections.Count > 0);
		Wall randWall = walls[Random.Range(0, walls.Count)];

		lastRandConDir = randWall.dir;

		return GetRandomConnection(randWall.possibleConnections);
	}
	private BoundsInt GetRandomConnection(VariantRegion varRegion, Direction dir)
	{
		RegionConnections conns = varRegion.connections.Find(x => x.direction == dir);
		return GetRandomConnection(conns.boundsList);
	}
	private BoundsInt GetRandomConnection(Region region, Direction dir)
	{
		Wall wall = region.walls.Find(x => x.dir == dir);
		return GetRandomConnection(wall.possibleConnections);
	}
	private BoundsInt GetRandomConnection(List<BoundsInt> possibleConnections)
	{
		BoundsInt connection = possibleConnections[Random.Range(0, possibleConnections.Count)];
		return connection;
	}

	private void CreateTileMap()
    {
		// Iterate over each region and instantiate it's tiles
		for (int i = 0; i < regions.Count; i++)
		{
			Region r = regions[i];
			BoundsInt ob = r.outerBounds;
			BoundsInt ib = r.innerBounds;
			Vector3Int basePosWalls = ob.min;
			Vector3Int basePosFloor = ib.min;

			Tileset tsFloor = (r.floorTiles != null) ? r.floorTiles : floorTiles;
			Tileset tsWall = (r.wallTiles != null) ? r.wallTiles : wallTiles;

			// Region Walls
			for (int x = 0; x < ob.size.x; x++)
			{
				for (int y = 0; y < ob.size.y; y++)
				{
					GameObject t = null;

					// Wall Corners
					if (x == 0 && y == 0)
						t = tsWall.BottomLeft;
					else if (x == ob.size.x - 1 && y == 0)
						t = tsWall.BottomRight;
					else if (x == 0 && y == ob.size.y - 1)
						t = tsWall.TopLeft;
					else if (x == ob.size.x - 1 && y == ob.size.y - 1)
						t = tsWall.TopRight;

					// Wall Sides
					else if (x == 0)
						t = tsWall.Left;
					else if (x == ob.size.x - 1)
						t = tsWall.Right;
					else if (y == 0)
						t = tsWall.Bottom;
					else if (y == ob.size.y - 1)
						t = tsWall.Top;

					// Wall Middle
					else if (x < Wall.thicknessWest || x > ob.size.x - Wall.thicknessEast - 1
						  || y < Wall.thicknessSouth || y > ob.size.y - Wall.thicknessNorth - 1)
						t = tsWall.Middle;

					else continue;

					SpawnTile(basePosWalls, new Vector2Int(x, y), t);
				}
			}

			for (int x = 0; x < ib.size.x; x++)
			{
				for (int y = 0; y < ib.size.y; y++)
				{
					GameObject t = null;

					// Floor Corners
					if (x == 0 && y == 0)
						t = tsFloor.BottomLeft;
					else if (x == ib.size.x - 1 && y == 0)
						t = tsFloor.BottomRight;
					else if (x == 0 && y == ib.size.y - 1)
						t = tsFloor.TopLeft;
					else if (x == ib.size.x - 1 && y == ib.size.y - 1)
						t = tsFloor.TopRight;

					// Floor Sides
					else if (x == 0)
						t = tsFloor.Left;
					else if (x == ib.size.x - 1)
						t = tsFloor.Right;
					else if (y == 0)
						t = tsFloor.Bottom;
					else if (y == ib.size.y - 1)
						t = tsFloor.Top;

					// Floor Middle
					else t = tsFloor.Middle;

					SpawnTile(basePosFloor, new Vector2Int(x, y), t);
				}
			}

			for (int j = 0; j < r.connections.Count; j++)
			{
				BoundsInt c = r.connections[j];
				Vector3Int conBasePos = c.min;
				for (int x = 0; x < c.size.x; x++)
				{
					for (int y = 0; y < c.size.y; y++)
					{
						SpawnTile(conBasePos, new Vector2Int(x, y), connectionTiles.Middle, true);
					}
				}
			}
		}
    }

	private void SpawnTile (Vector3 basePosition, Vector2Int relativePosition, GameObject tilePrefab, bool overrideExisiting = false)
	{
		// Tile Size
		Vector3 size = Vector3.Scale(Vector3.one, new Vector3(tileSize, tileSize, 1f));
		Vector2Int mapPos = GetPositionInMap(basePosition) + relativePosition;
		Vector3 pos = new Vector3(generationBounds.xMin + (mapPos.x + 0.5f) * tileSize,
			generationBounds.yMin + (mapPos.y + 0.5f) * tileSize);

		if (tileMap[mapPos.x, mapPos.y] != null)
		{
			if (overrideExisiting)
			{
				Destroy(tileMap[mapPos.x, mapPos.y]);
			}
			else
			{
				//Debug.LogWarning("Spawn Tile - " + "Tile spot already taken at " + mapPos.ToString() + "! (" + tilePrefab.name.ToString() + " -> " + tileMap[mapPos.x, mapPos.y].name.ToString() + ")");
				return;
			}
		}

		tileMap[mapPos.x, mapPos.y] = Instantiate(tilePrefab, pos, Quaternion.identity, tileParent.transform);
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
        if (tileParent != null)
        {
            Destroy(tileParent);
            tileParent = null;
        }
        tileParent = new GameObject("Tile Map");
		if (furnitureParent != null)
        {
            Destroy(furnitureParent);
			furnitureParent = null;
        }
		furnitureParent = new GameObject("Obstacles");

		// Reset region ids
		Region.NEXT_ID = 0;

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
				regionMap[x, y] = new RegionSpot(RegionType.None, -1);
				tileMap[x, y] = null;
            }
        }

		numAddRoom = 0;
		numAddCor = 0;
    }

    private void OnDrawGizmos()
    {
        if (regions != null && regions.Count > 0)
        {
            foreach (Region r in regions)
            {
				foreach (Wall w in r.walls)
				{
					Gizmos.color = Color.red;
					Gizmos.DrawWireCube(w.bounds.center, w.bounds.size);

					foreach (BoundsInt pCon in w.possibleConnections)
					{
						Gizmos.color = Color.cyan;
						Gizmos.DrawWireCube(pCon.center, pCon.size - new Vector3(0.2f, 0.2f));
					}
				}
				foreach (BoundsInt b in r.connections)
				{
					Gizmos.color = Color.magenta;
					Gizmos.DrawWireCube(b.center, b.size);
				}

				Gizmos.color = Color.black;
				Bounds outer = new Bounds(r.outerBounds.center, r.outerBounds.size + new Vector3(0.1f, 0.1f));
				Gizmos.DrawWireCube(outer.center, outer.size);

				Gizmos.color = Color.green;
				Bounds inner = new Bounds(r.innerBounds.center, r.innerBounds.size - new Vector3(0.1f, 0.1f));
				Gizmos.DrawWireCube(inner.center, inner.size);

			}
		}
		if (DebugBounds != null)
		{
			Color color = Color.red;
			color.a = 0.5f;
			Gizmos.color = color;
			Gizmos.DrawCube(DebugBounds.center, DebugBounds.size);
		}
	}
}
