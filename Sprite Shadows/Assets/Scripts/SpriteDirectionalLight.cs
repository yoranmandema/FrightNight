using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteDirectionalLight : SpriteLightCaster {
    public float Angle = 35;

    public override List<GameObject> GetTilesInRange() {
        var cast = Physics2D.CircleCastAll(transform.position, Distance, Vector2.up, LayerMask.GetMask("Floor"));
        var tiles = new List<GameObject>();

        for (var i = 0; i < cast.Length; i++) {
            var gameObject = cast[i].collider.gameObject;

            var direction = (transform.position - gameObject.transform.position).normalized;

            var angle = Vector3.Angle(transform.up, direction);

            if (angle < Angle)
                tiles.Add(gameObject);
        }

        return tiles;
    }

    public override void Illuminate() {

        var cast = Physics2D.CircleCastAll(transform.position, Distance, Vector2.up, LayerMask.GetMask("Floor"));

        for (var i = 0; i < cast.Length; i++) {
            var t = cast[i].collider.gameObject;

            float inDirectionMultiplier = 0;

            for (var x = 0; x < ShadowFiltering; x++) {
                for (var y = 0; y < ShadowFiltering; y++) {
                    var offset = new Vector3(x - ShadowFiltering * 0.5f + 0.5f, y - ShadowFiltering * 0.5f + 0.5f,0) / ShadowFiltering / 2;
                    var direction = (transform.position - (t.transform.position + offset)).normalized;

                    var angle = Vector3.Angle(transform.up, direction);

                    if (angle < Angle)
                        inDirectionMultiplier += 1/ (float)(ShadowFiltering*ShadowFiltering);
                }
            }

            if (inDirectionMultiplier == 0) continue; 

            var r = t.GetComponent<SpriteLightReciever>();
            var distanceToReciever = Vector2.Distance(t.transform.position, transform.position);
            var color = Color * (1 - distanceToReciever / Distance) * inDirectionMultiplier;

            if (CastShadows) {
                color = Shadow(color, t);

                if (color.grayscale == 0) {
                    continue;
                }
            }

            if (distanceToReciever <= Distance)
                r.Illuminate(color);
        }
    }
}
