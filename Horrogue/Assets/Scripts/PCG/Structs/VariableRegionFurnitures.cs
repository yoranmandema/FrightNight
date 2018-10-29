using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct VariableRegionFurnitures {
	public string name;
	public int spawnAmount;
	public GameObject prefab;
	public List<Vector3> spawnLocations;

	public VariableRegionFurnitures(string name)
	{
		this.name = name;
		this.prefab = null;
		this.spawnAmount = 0;
		spawnLocations = new List<Vector3>();
	}

	public VariableRegionFurnitures(GameObject prefab)
	{
		this.name = prefab.name;
		this.prefab = prefab;
		this.spawnAmount = 0;
		spawnLocations = new List<Vector3>();
	}

	public void AddPosition(Vector3 pos)
	{
		spawnLocations.Add(pos);
		spawnAmount++;
	}
}
