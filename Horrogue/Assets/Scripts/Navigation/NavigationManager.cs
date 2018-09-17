using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Collections;
using System.Diagnostics;

[ExecuteInEditMode]
public class NavigationManager : MonoBehaviour {

    public bool update;
    public int subDivision = 1;
    public List<Vector2> grid = new List<Vector2>();

    private List<GameObject> walkables = new List<GameObject>();

    void OnValidate() {
        GenerateNavigationGrid();
    }

    private List<GameObject> getWalkableSurfaces () {
        var objects = GameObject.FindGameObjectsWithTag("Walkable");

        return new List<GameObject>(objects);
    }

    /// <summary>
    /// Generates a list of points that NPC's can walk along based on existing 'walkable' objects.
    /// </summary>
    public void GenerateNavigationGrid () {
        var sw = Stopwatch.StartNew();

        walkables = getWalkableSurfaces();
        grid.Clear();

        for (int i = 0; i < walkables.Count; i++) {
            var w = walkables[i];

            for (var x = 0; x < subDivision; x++) {
                for (var y = 0; y < subDivision; y++) {
                    var offsetPosition = new Vector2(x - subDivision * 0.5f + 0.5f, y - subDivision * 0.5f + 0.5f) / subDivision * w.transform.lossyScale;
                    var resultPosition = (Vector2)w.transform.position + offsetPosition;

                    var collidesWithObject = Physics2D.OverlapCircleAll(resultPosition, 1 / subDivision);

                    // Only add grid point if its not colliding with anything
                    if (collidesWithObject.Length == 0)
                        grid.Add((Vector2)w.transform.position + offsetPosition);
                }
            }
        }

        sw.Stop();
        var timeTaken = sw.Elapsed.TotalMilliseconds;

        UnityEngine.Debug.LogFormat("Generated navigation grid in {0}ms", timeTaken);
    }
   
    void OnDrawGizmosSelected() {
        foreach (var v in grid) {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            Gizmos.DrawCube(v, new Vector2(0.1f, 0.1f));
        }
    }
}
