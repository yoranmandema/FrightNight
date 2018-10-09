using System;
using System.Collections.Generic;
using UnityEngine;

// Types of tiles that are available
public enum TileType
{
	Air,
	Ground,
	Wall,
	Doorway
}

// Container for sprites to be used for a certain tile type
[Serializable, CreateAssetMenu(fileName = "New Tile Sprite", menuName = "Procedural/Tile Sprite")]
public class TileSprites : ScriptableObject
{
	public TileSprites(TileType type)
	{
		this.tileType = type;
		this.tilePrefabs = new List<GameObject>(1);
	}

	public TileType tileType;
	public List<GameObject> tilePrefabs;
}