using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public float Scale = 2f;
    public float MaxDistance = 1f;
    public float Smoothness = 0.5f;

    private GameManager GameManager;
    private Vector3 desired;

    void Start () {
        GameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

	void FixedUpdate () {
        if (GameManager.ControlledObject == null) return;

        var cursorPos = Camera.main.ScreenToViewportPoint(Input.mousePosition - new Vector3(Screen.width,Screen.height,0)/2);
    

        desired = (GameManager.ControlledObject.transform.position + Vector3.ClampMagnitude(cursorPos, MaxDistance) * Scale) + new Vector3(0,0,-10);

        transform.position = transform.position * Smoothness + desired * (1 - Smoothness);
    }
}
