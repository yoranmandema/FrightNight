using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tileset", menuName = "Procedural")]
public class Tileset : ScriptableObject {

	public TileType type;
	public GameObject TopLeft, Top, TopRight,
		Left, Middle, Right,
		BottomLeft, Bottom, BottomRight;
	
}
