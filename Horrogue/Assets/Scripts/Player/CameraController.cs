using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public float Scale = 2f;
    public float MaxDistance = 1f;
    public float Smoothness = 0.5f;
    public GameObject Player;

    private Vector3 desired;

	void Update () {
        var cursorPos = Camera.main.ScreenToViewportPoint(Input.mousePosition - new Vector3(Screen.width,Screen.height,0)/2);
        var direction = (cursorPos - transform.position).normalized;

        desired = Vector3.ClampMagnitude(cursorPos, MaxDistance) * Scale + new Vector3(0,0,-10);

        transform.localPosition = transform.localPosition * Smoothness + desired * (1 - Smoothness);
    }
}
