using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tileset", menuName = "Procedural/New Tileset")]
public class Tileset : ScriptableObject {

	public GameObject TopLeft, Top, TopRight,
		Left, Middle, Right,
		BottomLeft, Bottom, BottomRight;
	
}
