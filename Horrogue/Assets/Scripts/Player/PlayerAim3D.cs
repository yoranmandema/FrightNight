using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAim3D : MonoBehaviour {

    public GameObject Light;
	
	// Update is called once per frame
	void Update () {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit rayHit;
        var rayCast = Physics.Raycast(ray.origin, ray.direction, out rayHit, Mathf.Infinity, ~LayerMask.GetMask("Player"));

        Light.transform.LookAt(rayHit.point);
    }
}
