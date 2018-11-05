using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct LocalTransform
{
	public Vector3 position;
	public Quaternion rotation;
	public Vector3 scale;
}

[Serializable]
public class RegionFurnitures {

	public string name;
	public GameObject prefab;
	public List<LocalTransform> spawnTransforms;

	public RegionFurnitures(string name)
	{
		this.name = name;
		this.prefab = null;
		spawnTransforms = new List<LocalTransform>();
	}

	public RegionFurnitures(GameObject prefab)
	{
		this.name = prefab.name;
		this.prefab = prefab;
		spawnTransforms = new List<LocalTransform>();
	}

	virtual internal void AddPosition(Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
	{
		LocalTransform transform = new LocalTransform();
		transform.position = localPosition;
		transform.rotation = localRotation;
		transform.scale = localScale;

		spawnTransforms.Add(transform);
	}

	public override string ToString()
	{
		return name + " Prefab: " + prefab.ToString() + " Spawn Transforms (Count): " + spawnTransforms.Count;
	}
}
