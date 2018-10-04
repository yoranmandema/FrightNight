using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightAim : MonoBehaviour {

    public LOS.LOSRadialLight Light;
    public AnimationController AnimationController;
    public float Smoothness = 0.5f;

    private PlayerMovement movement;
    private Quaternion desiredRotation;
    private Quaternion smoothedRotation;

    private float AngleTo(Vector2 pos, Vector2 target) {
        Vector2 difference = target - pos;
        float sign = (target.y < pos.y) ? -1.0f : 1.0f;
        return Vector2.Angle(Vector2.right, difference) * sign;
    }

    private void Start()
    {
        movement = GetComponent<PlayerMovement>();
    }

    void Update () {
	    var cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - new Vector3(0,0,Camera.main.transform.position.z);
        var direction = cursorPos - Light.transform.position;
        direction.Normalize();

        float rot_z = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        desiredRotation = Quaternion.Euler(0f, 0f, rot_z);

        smoothedRotation = Quaternion.Slerp(desiredRotation, smoothedRotation, Smoothness);

        var angle = smoothedRotation.eulerAngles.z;

        Light.faceAngle = angle;

        if (!movement.IsMoving)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                if (direction.x > 0)
                {
                    AnimationController.Direction = "Right";
                }
                else
                {
                    AnimationController.Direction = "Left";
                }
            }
            else
            {
                if (direction.y > 0)
                {
                    AnimationController.Direction = "Up";
                }
                else
                {
                    AnimationController.Direction = "Down";
                }
            }

            AnimationController.Play("Idle");
        }
    }
}
