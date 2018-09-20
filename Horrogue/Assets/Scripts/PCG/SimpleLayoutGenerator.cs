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
        public TileType tileType;
        public List<Sprite> sprites;
    }

	#region Public Variables
	public bool useRandomSeed;
	public string seed = "elementary";
	public BoundsInt generationBounds;
    public int tileSize = 1;
    public List<TileSprites> tileSprites;
	#endregion

	#region Private Variables
	private int[,] map;
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

		PlaceCorridors();
		PlaceRooms();
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
		map = new int[generationBounds.size.x, generationBounds.size.y];
		for (int x = 0; x < generationBounds.size.x; x++)
		{
			for (int y = 0; y < generationBounds.size.y; y++)
			{
				map[x, y] = (int) TileType.Air;
			}
		}
	}

    private void PlaceCorridors()
    {
        // First place a main corridor if none are placed
        // Some temporary parameters
        int mainCorridorWidth = 7;
        int mainCorridorHalf = 4;

        // Pick random spot inside given bounds and extend a corridor in one direction to its maximum length
        // Also give the generation some padding so the corridor generates with it's full width
        Vector2Int mainCorridorSpawn = new Vector2Int(Random.Range(mainCorridorHalf, generationBounds.size.x - mainCorridorHalf), 
            Random.Range(mainCorridorHalf, generationBounds.size.y - mainCorridorHalf));

        // Temporary direction choosing, 0 is horizontal, 1 is vertical
        bool corridorVert = Mathf.RoundToInt(Random.value) == 1;

        int aMin = 0;
        int aMax = 0;
        int bMin = 0;
        int bMax = 0;
        if (corridorVert) {
            aMin = mainCorridorSpawn.x - mainCorridorHalf;
            aMax = mainCorridorSpawn.x + mainCorridorHalf;

            bMin = 0 + mainCorridorHalf;
            bMax = generationBounds.size.y - mainCorridorHalf;
        }
        else {
            aMin = 0 + mainCorridorHalf;
            aMax = generationBounds.size.x - mainCorridorHalf;

            bMin = mainCorridorSpawn.y - mainCorridorHalf;
            bMax = mainCorridorSpawn.y + mainCorridorHalf;
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

    private void PlaceRooms()
	{
		//throw new NotImplementedException();
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
        }
    }
}
