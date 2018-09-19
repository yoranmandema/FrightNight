using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntersectionTest : MonoBehaviour {

    public GameObject tile;

    // Update is called once per frame
    void Update () {
        Vector2 min = (Vector2)tile.transform.position + new Vector2(-0.5f, 0.5f);
        Vector2 max = (Vector2)tile.transform.position + new Vector2(0.5f, 0.5f);
        Vector2 min2 = (Vector2)tile.transform.position + new Vector2(0.5f, -0.5f);
        Vector2 max2 = (Vector2)tile.transform.position + new Vector2(-0.5f, -0.5f);

        Vector2 origin = Vector2.zero;
        Vector2 camMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (camMouse - origin).normalized;
        Vector2 intersection;

        bool hits = TwoDee.Intersection.LineSegmentBoxIntersection(
            origin,
            camMouse,
            max2,
            min2,
            max,
            min,
            out intersection
            );

        Debug.DrawLine(origin, intersection);
    }
}
