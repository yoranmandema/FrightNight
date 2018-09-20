using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SimpleLayoutGenerator : MonoBehaviour {

	public enum TileType
	{
		Nothing,
		Ground,
		Wall,
	}

	#region Public Variables
	public bool useRandomSeed;
	public string seed = "elementary";
	public BoundsInt generationBounds;
	#endregion

	#region Private Variables
	private int[,] map;
	#endregion

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
		map = new int[generationBounds.xMax, generationBounds.yMax];
		for (int x = generationBounds.xMin; x < generationBounds.xMax; x++)
		{
			for (int y = generationBounds.yMin; y < generationBounds.yMax; y++)
			{
				map[x, y] = 0;
			}
		}
	}

	private void PlaceRooms()
	{
		throw new NotImplementedException();
	}

	private void PlaceCorridors()
	{
		throw new NotImplementedException();
	}

}
