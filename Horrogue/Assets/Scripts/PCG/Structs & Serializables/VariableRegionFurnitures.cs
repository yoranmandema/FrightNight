using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VariableRegionFurnitures : RegionFurnitures {
	public Range spawnAmount;

	public VariableRegionFurnitures(string name) : base(name)
	{
		this.spawnAmount = new Range(0, 0);
	}

	public VariableRegionFurnitures(GameObject prefab) : base(prefab)
	{
		this.spawnAmount = new Range(0, 0);
	}

	internal override void AddPosition(Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
	{
		base.AddPosition(localPosition, localRotation, localScale);
		this.spawnAmount.min++;
		this.spawnAmount.max++;
	}
}
