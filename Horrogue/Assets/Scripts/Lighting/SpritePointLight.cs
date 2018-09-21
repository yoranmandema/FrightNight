using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpritePointLight : SpriteLightCaster {
    public override List<GameObject> GetTilesInRange() {
        var cast = Physics2D.CircleCastAll(transform.position, Distance, Vector2.zero, LayerMask.GetMask("Floor"));
        var tiles = new List<GameObject>();

        for (var i = 0; i < cast.Length; i++) {
            if (cast[i].collider.gameObject.GetComponent<SpriteLightReciever>() != null) 
                tiles.Add(cast[i].collider.gameObject);
        }

        return tiles;
    }
}
