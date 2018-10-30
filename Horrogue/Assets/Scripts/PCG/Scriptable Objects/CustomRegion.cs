using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Custom Region", menuName = "Procedural/New Custom Region")]
public class CustomRegion : ScriptableObject {
	public List<Tileset> tilesets;
	public RegionType type;
	public List<VariantRegion> regionVariations;

	public bool seperateVerticalRegions = false;
	public List<VariantRegion> verticalRegionVariations;
}
