using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightAim : MonoBehaviour {

    public LOS.LOSRadialLight Shadow;
    public LOS.LOSRadialLight Light;

    private float AngleTo(Vector2 pos, Vector2 target) {
        Vector2 diference = target - pos;
        float sign = (target.y < pos.y) ? -1.0f : 1.0f;
        return Vector2.Angle(Vector2.right, diference) * sign;
    }

    void Update () {
	    var cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var direction = (cursorPos - transform.position).normalized;

        var angle = AngleTo(transform.position, cursorPos);

        Shadow.faceAngle = angle + 180;
        Light.faceAngle = angle;
    }
}
