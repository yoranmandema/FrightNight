using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SpriteLightCaster : MonoBehaviour {

    public float Distance = 3;
    public Color Color = Color.white;
    public bool CastShadows = false;
    public int ShadowFiltering = 2;

    public virtual void Illuminate() {
        var tiles = GetTilesInRange();

        foreach (var t in tiles) {
            var r = t.GetComponent<SpriteLightReciever>();
            var distanceToReciever = Vector2.Distance(t.transform.position, transform.position);
            var color = Color * (1 - distanceToReciever / Distance);

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

    public virtual List<GameObject> GetTilesInRange() {
        return new List<GameObject>();
    }

    void OnValidate() {
        Illuminate();
    }

    void Update() {
        Illuminate();
    }

    public Color Shadow(Color reference, GameObject tile) {
        Color shadowColor = Color.black;

        for (var x = 0; x < ShadowFiltering; x++) {
            for (var y = 0; y < ShadowFiltering; y++) {
                Vector2 offset =
                    new Vector2(x - ShadowFiltering * 0.5f + 0.5f, y - ShadowFiltering * 0.5f + 0.5f) / ShadowFiltering / 2;

                var shadowCast = Physics2D.Linecast((Vector2)tile.transform.position + offset, transform.position, LayerMask.GetMask("Wall"));

                if (shadowCast.collider != null) {
                    if (shadowCast.collider.gameObject == tile) {
                        shadowColor = reference* ShadowFiltering;
                    } else {
                        shadowColor += Color.black;
                    }
                }
                else {
                    shadowColor += reference;
                }
            }
        }

        return shadowColor / (ShadowFiltering * ShadowFiltering);
    }
}
 