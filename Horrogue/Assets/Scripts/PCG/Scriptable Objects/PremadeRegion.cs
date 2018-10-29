using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(fileName = "Premade Region", menuName = "Procedural/New Premade Region")]
public class PremadeRegion : ScriptableObject
{
	public bool isSpawn;
	public RegionType type;

	public int innerRegionWidth;
	public int innerRegionLength;

	public List<RegionFurnitures> furnitures;
	public List<RegionConnections> connections;
	public Tileset tileset;

	public void Init(string name, int innerRegionWidth, int innerRegionLength, List<RegionFurnitures> furnitures, List<RegionConnections> connections, Tileset tileset)
	{
		this.name = name;
		this.innerRegionWidth = innerRegionWidth;
		this.innerRegionLength = innerRegionLength;
		this.furnitures = new List<RegionFurnitures>(furnitures);
		this.connections = new List<RegionConnections>(connections);
		this.tileset = tileset;
	}
}
