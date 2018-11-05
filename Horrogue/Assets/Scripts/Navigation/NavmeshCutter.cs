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
            if (lastBounds != null)
                AstarPath.active.UpdateGraphs(new GraphUpdateObject(lastBounds));

            var bounds = GetComponent<Collider2D>().bounds;

            bounds.center = new Vector3(bounds.center.x, bounds.center.y, 10);
            // Expand the bounds along the Z axis
            bounds.Expand(Vector3.forward * 1000);

            // change some settings on the object
            AstarPath.active.UpdateGraphs(new GraphUpdateObject(bounds));

            lastBounds = bounds;
        }
    }

    void OnDrawGizmos () {
        Gizmos.DrawCube(lastBounds.center, lastBounds.size);
    }
}
