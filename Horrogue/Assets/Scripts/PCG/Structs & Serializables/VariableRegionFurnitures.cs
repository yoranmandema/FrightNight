using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VariableRegionFurnitures : RegionFurnitures {
	public int spawnAmount;

	public VariableRegionFurnitures(string name) : base(name)
	{
		this.spawnAmount = 0;
	}

	public VariableRegionFurnitures(GameObject prefab) : base(prefab)
	{
		this.spawnAmount = 0;
	}

	internal override void AddPosition(Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
	{
		base.AddPosition(localPosition, localRotation, localScale);
		this.spawnAmount++;
	}
}
