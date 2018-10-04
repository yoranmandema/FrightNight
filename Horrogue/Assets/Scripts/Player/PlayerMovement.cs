using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    public float MaxVelocity;
    public float Accelaration;
    public Animator Animator;
    public AnimationController AnimationController;

    public bool IsMoving;
    private Rigidbody2D rb;

	void Start () {
        rb = GetComponent<Rigidbody2D>();
    }

	void FixedUpdate () {
        rb.velocity += new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized * Accelaration;

        rb.velocity = Vector3.ClampMagnitude(rb.velocity, MaxVelocity);

        if (rb.velocity.magnitude > 0.25f)
        {
            if (Mathf.Abs(rb.velocity.x) > 0.1f)
            {
                if (rb.velocity.x > 0)
                {
                    AnimationController.Direction = "Right";
                }
                else
                {
                    AnimationController.Direction = "Left";
                }

                AnimationController.Play("Walk");
            }
            else
            {
                if (rb.velocity.y > 0)
                {
                    AnimationController.Direction = "Up";
                }
                else
                {
                    AnimationController.Direction = "Down";
                }

                AnimationController.Play("Walk");
            }

            IsMoving = true;
        }
        else { 
            IsMoving = false;
        }
    }
}
