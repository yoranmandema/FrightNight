using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectThrower : MonoBehaviour {

    public GameObject Object;
    public LightAim Aim;

    void Start () {
        Aim = GetComponent<LightAim>();
    }

	void Update () {
        if (Input.GetButtonDown("Fire1")) {
            Instantiate(Object, transform.position, Quaternion.Euler(0, 0, Aim.Rotation));
        }
    }
}
