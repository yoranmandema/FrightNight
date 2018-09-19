using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using TwoDee;

public struct SpriteIlluminationJob : IJobParallelFor {
    public NativeArray<Color> Colors;
    public Vector2 LightPos;
    public NativeArray<Vector2> TilePositions;
    public bool CastShadows;
    public int ShadowFiltering;
    public LayerMask LayerMask;

    public void Execute(int index) {
        var color = Colors[index];

        var shadowColor = Color.black;
        var tilePos = TilePositions[index];

        for (var x = 0; x < ShadowFiltering; x++) {
            for (var y = 0; y < ShadowFiltering; y++) {
                Vector2 offset =
                    new Vector2(x - ShadowFiltering * 0.5f + 0.5f, y - ShadowFiltering * 0.5f + 0.5f) / ShadowFiltering / 2;

                bool isOccluded = false;

                for (var i = 0; i < Colors.Length; i++) {
                    if (index == i) continue;

                    var p1 = TilePositions[i] + new Vector2(-0.5f,0.5f);
                    var p2 = TilePositions[i] + new Vector2(0.5f,0.5f);
                    var p3 = TilePositions[i] + new Vector2(0.5f,-0.5f);
                    var p4 = TilePositions[i] + new Vector2(-0.5f, -0.5f);

                    var hitResult = Vector2.zero;

                    var hits = Intersection.LineSegmentBoxIntersection(tilePos + offset, LightPos, p1, p2, p3, p4, out hitResult);

                    if (hits) {
                        isOccluded = true;
                        break;
                    }
                }

                if (isOccluded) {
                    shadowColor += Color.black;
                }
                else {
                    shadowColor += color;
                }
            }
        }

        if (color.grayscale == 0) return;

        color = shadowColor / (ShadowFiltering * ShadowFiltering);

        Colors[index] = color;
    }
}
