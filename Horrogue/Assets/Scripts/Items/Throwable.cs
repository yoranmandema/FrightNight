using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throwable : MonoBehaviour {

    #region Public Variables
    public float Velocity = 0.5f;

    public float UpVelocity = 0.5f;
    public float Gravity = 0.25f;
    public float Bounce = 0.5f;

    public bool StopAfterBounce = true;

    public GameObject Sprite;
    #endregion

    #region Private Variables
    private float verticalSpeed;
    private float height;
    private Rigidbody2D rb;
    #endregion

    void Start () {
        rb = GetComponent<Rigidbody2D>();

        verticalSpeed = UpVelocity;
    }
	
    public void Throw (Vector3 direction) {
        GetComponent<Rigidbody2D>().velocity = direction * Velocity;
    }

    void FixedUpdate () {
        if (Sprite == null) return; 

        if (height >= 0) {
            verticalSpeed += -Gravity;
        } 

        height += verticalSpeed;

        if (height < 0) {
            height = -height;

            if (verticalSpeed < 0) verticalSpeed = -verticalSpeed * Bounce;// - 0.7f;

            if (verticalSpeed < 0.01f) {
                height = 0;
                verticalSpeed = 0;

                rb.velocity *= 0;
            }
        }

        var currentPos = Sprite.transform.position;
        currentPos.y = gameObject.transform.position.y + height;

        Sprite.transform.position = currentPos;
    }
}
