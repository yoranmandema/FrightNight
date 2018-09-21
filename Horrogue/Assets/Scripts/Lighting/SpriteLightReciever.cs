using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteLightReciever : MonoBehaviour {
    List<Color> lightColors = new List<Color>();
    Renderer r;

    void Start () {
        r = GetComponent<Renderer>();
    }

    void Update() {

        if (lightColors.Count == 0) {
            r.material.color = Color.black;
        } else {
            var averageColor = Color.black;

            for (var i = 0; i < lightColors.Count; i++) {
                averageColor += lightColors[i];
            }

            //averageColor /= lightColors.Count;

            averageColor = new Color(
                Mathf.Clamp(averageColor.r, 0, 2f),
                Mathf.Clamp(averageColor.g, 0, 2f),
                Mathf.Clamp(averageColor.b, 0, 2f),
                1
            );

            r.material.color = averageColor;
        }

        lightColors.Clear();
    }

    public void Illuminate(Color color) {
        var newColor = new Color(
                Mathf.Clamp(color.r, 0, 10f),
                Mathf.Clamp(color.g, 0, 10f),
                Mathf.Clamp(color.b, 0, 10f),
                1
            );

        lightColors.Add(newColor);
    }
}
