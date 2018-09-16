using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAim : MonoBehaviour {

    public GameObject Light;
    public GameObject Cam;

    public float LightRotateSpeed;

	void Start () {
		
	}
	
	void Update () {
        SetLightRotation();
    }

    void SetLightRotation () {
        //Get the Screen positions of the object
        var positionOnScreen = Camera.main.WorldToViewportPoint(Light.transform.position);

        //Get the Screen position of the mouse
        var mouseOnScreen = (Vector2)Camera.main.ScreenToViewportPoint(Input.mousePosition);

        //Get the angle between the points
        var angle = AngleBetweenTwoPoints(positionOnScreen, mouseOnScreen);

        var newRotation = Quaternion.Slerp(Light.transform.rotation, Quaternion.Euler(new Vector3(0f, 0f, angle)),LightRotateSpeed * Time.deltaTime);

        //Ta Daaa
        Light.transform.rotation = newRotation;
    }

    float AngleBetweenTwoPoints(Vector3 a, Vector3 b) {
        return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
    }
}
