using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    public float MaxVelocity;
    public float Accelaration;

    private Rigidbody2D rb;

	void Start () {
        rb = GetComponent<Rigidbody2D>();
    }

	void FixedUpdate () {
        rb.velocity += new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized * Accelaration;

        rb.velocity = Vector3.ClampMagnitude(rb.velocity, MaxVelocity);
    }
}
