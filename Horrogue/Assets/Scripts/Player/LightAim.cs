using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightAim : MonoBehaviour {

    public LOS.LOSRadialLight Light;
    private PlayerMovement movement;

    private float AngleTo(Vector2 pos, Vector2 target) {
        Vector2 diference = target - pos;
        float sign = (target.y < pos.y) ? -1.0f : 1.0f;
        return Vector2.Angle(Vector2.right, diference) * sign;
    }

    private void Start()
    {
        movement = GetComponent<PlayerMovement>();
    }

    void Update () {
	    var cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var direction = (cursorPos - transform.position).normalized;

        var angle = AngleTo(transform.position, cursorPos);

        Light.faceAngle = angle;

        if (!movement.IsMoving)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                if (direction.x > 0)
                {
                    movement.PlayAnimation("Idle", "Right");
                }
                else
                {
                    movement.PlayAnimation("Idle", "Left");
                }
            }
            else
            {
                if (direction.y > 0)
                {
                    movement.PlayAnimation("Idle", "Up");
                }
                else
                {
                    movement.PlayAnimation("Idle", "Down");
                }
            }
        }
    }
}
