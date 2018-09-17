using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

[ExecuteInEditMode]
public class NavmeshCutter : MonoBehaviour {

    Bounds lastBounds;

	// Update is called once per frame
	void Update () {
        CutNavmesh();
    }

    void OnValidate() {
        CutNavmesh();
    }

    public void CutNavmesh () {
        if (AstarPath.active != null) {
            if (lastBounds != null) {
                // change some settings on the object
                AstarPath.active.UpdateGraphs(new GraphUpdateObject(lastBounds));
            }

            var bounds = GetComponent<Collider2D>().bounds;
            // Expand the bounds along the Z axis
            bounds.Expand(Vector3.forward * 1000);
            var guo = new GraphUpdateObject(bounds);
            // change some settings on the object
            AstarPath.active.UpdateGraphs(guo);

            lastBounds = bounds;
        }
    }
}
