using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement3D : MonoBehaviour {

    public float MaxVelocity;
    public float Accelaration;

    private CharacterController controller;
    private Vector3 velocity;

    void Start() {
        controller = GetComponent<CharacterController>();
    }

    void FixedUpdate() {
        var forwardVector = Camera.main.transform.forward;
        forwardVector.y = 0;
        forwardVector.Normalize();

        var rightVector = Camera.main.transform.right;

        velocity -= velocity * Accelaration;
        velocity +=
            (forwardVector * Input.GetAxis("Vertical") + rightVector * Input.GetAxis("Horizontal")).normalized * Accelaration;

        velocity = Vector3.ClampMagnitude(velocity, MaxVelocity);

        controller.Move(velocity);
    }
}
