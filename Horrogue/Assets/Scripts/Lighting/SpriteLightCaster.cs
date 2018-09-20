using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

[ExecuteInEditMode]
public class SpriteLightCaster : MonoBehaviour
{

    public float Distance = 3;
    public Color Color = Color.white;
    public bool CastShadows = false;
    public int ShadowFiltering = 2;
    public bool UseJobSystem = false;

    public virtual void Illuminate()
    {
        var tiles = GetTilesInRange();

        if (!UseJobSystem)
        {
            foreach (var t in tiles)
            {
                var r = t.GetComponent<SpriteLightReciever>();
                var distanceToReciever = Vector2.Distance(t.transform.position, transform.position);
                var color = Color * (1 - distanceToReciever / Distance);

                if (CastShadows)
                {
                    color = Shadow(color, t);

                    if (color.grayscale == 0)
                    {
                        continue;
                    }
                }

                if (distanceToReciever <= Distance)
                    r.Illuminate(color);
            }
        }
        else
        {
            //var colors = new List<Color>();
            //var tilePositions = new List<Vector2>();
            //var count = tiles.Count;

            //foreach (var t in tiles)
            //{
            //    var distanceToReciever = Vector2.Distance(t.transform.position, transform.position);
            //    var color = Color * (1 - distanceToReciever / Distance);
            //    colors.Add(color);
            //    tilePositions.Add(t.transform.position);
            //}

            //var colorArray = new NativeArray<Color>(colors.ToArray(), Allocator.TempJob);
            //var positionArray = new NativeArray<Vector2>(tilePositions.ToArray(), Allocator.TempJob);

            //var job = new SpriteIlluminationJob
            //{
            //    Colors = colorArray,
            //    LightPos = transform.position,
            //    TilePositions = positionArray,
            //    CastShadows = CastShadows,
            //    ShadowFiltering = ShadowFiltering,
            //    LayerMask = LayerMask.GetMask("Wall")
            //};

            //var jobhandle = job.Schedule(count, 250);
            //jobhandle.Complete();

            //var newColors = new Color[count];
            //colorArray.CopyTo(newColors);
            //colorArray.Dispose();
            //positionArray.Dispose();

            //for (var i = 0; i < count; i++)
            //{
            //    tiles[i].GetComponent<SpriteLightReciever>().Illuminate(newColors[i]);
            //}
        }
    }

    public virtual List<GameObject> GetTilesInRange()
    {
        return new List<GameObject>();
    }

    void OnValidate()
    {
        Illuminate();
    }

    void Update()
    {
        Illuminate();
    }

    public Color Shadow(Color reference, GameObject tile)
    {
        Color shadowColor = Color.black;

        for (var x = 0; x < ShadowFiltering; x++)
        {
            for (var y = 0; y < ShadowFiltering; y++)
            {
                Vector2 offset =
                    new Vector2(x - ShadowFiltering * 0.5f + 0.5f, y - ShadowFiltering * 0.5f + 0.5f) / ShadowFiltering / 2;

                var shadowCast = Physics2D.Linecast((Vector2)tile.transform.position + offset, transform.position, LayerMask.GetMask("Wall"));

                if (shadowCast.collider != null)
                {
                    if (shadowCast.collider.gameObject == tile)
                    {
                        shadowColor = reference * ShadowFiltering;
                    }
                    else
                    {
                        shadowColor += Color.black;
                    }
                }
                else
                {
                    shadowColor += reference;
                }
            }
        }

        return shadowColor / (ShadowFiltering * ShadowFiltering);
    }
}
