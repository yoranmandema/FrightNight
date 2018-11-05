using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Variant Region", menuName = "Procedural/New Variant Region")]
public class VariantRegion : PremadeRegion {

	public List<VariableRegionFurnitures> variableFurnitures;

	public VariantRegion(PremadeRegion region)
	{
		this.name = region.name;
		this.innerRegionWidth = region.innerRegionWidth;
		this.innerRegionLength = region.innerRegionLength;
		this.furnitures = new List<RegionFurnitures>(region.furnitures);
		this.connections = new List<RegionConnections>(region.connections);
		this.tileset = region.tileset;
		this.variableFurnitures = new List<VariableRegionFurnitures>();
	}

	public void Init(string name, int innerRegionWidth, int innerRegionLength, List<RegionFurnitures> furnitures, List<RegionConnections> connections, Tileset tileset, List<VariableRegionFurnitures> variableFurnitures)
	{
		base.Init(name, innerRegionWidth, innerRegionLength, furnitures, connections, tileset);

		this.variableFurnitures = new List<VariableRegionFurnitures>(variableFurnitures);
	}
}
