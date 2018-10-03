using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    public float MaxVelocity;
    public float Accelaration;
    public Animator Animator;

    public bool IsMoving;
    private Rigidbody2D rb;
    private string playerAnimationDirection = "Left";

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
                    playerAnimationDirection = "Right";
                    PlayAnimation("Walk", playerAnimationDirection);
                }
                else
                {
                    playerAnimationDirection = "Left";
                    PlayAnimation("Walk", playerAnimationDirection);
                }
            }
            else
            {
                if (rb.velocity.y > 0)
                {
                    playerAnimationDirection = "Up";
                    PlayAnimation("Walk", playerAnimationDirection);
                }
                else
                {
                    playerAnimationDirection = "Down";
                    PlayAnimation("Walk", playerAnimationDirection);
                }
            }

            IsMoving = true;
        }
        else { 
            IsMoving = false;
        }
    }

    public void PlayAnimation (string name, string direction) {
        Animator.Play(name + " " + direction, 0);
    }
}
