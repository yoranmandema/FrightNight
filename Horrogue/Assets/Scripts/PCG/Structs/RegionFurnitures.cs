using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct RegionFurnitures {
	public string name;
	public GameObject prefab;
	public List<Vector3> positions;

	public RegionFurnitures(string name)
	{
		this.name = name;
		this.prefab = null;
		positions = new List<Vector3>();
	}

	public RegionFurnitures(GameObject prefab)
	{
		this.name = prefab.name;
		this.prefab = prefab;
		positions = new List<Vector3>();
	}

	public void AddPosition(Vector3 pos)
	{
		positions.Add(pos);
	}
}
