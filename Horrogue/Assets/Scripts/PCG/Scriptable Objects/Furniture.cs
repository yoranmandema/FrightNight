using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(fileName = "New Furniture", menuName = "Procedural Content/Furniture")]
public class Furniture : ScriptableObject
{
	public GameObject prefab;
	public Vector2Int facing;
}