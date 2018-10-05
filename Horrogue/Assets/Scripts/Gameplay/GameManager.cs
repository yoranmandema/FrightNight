using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public GameObject ControlledObject;
    public GameObject CameraObject;

	void Start () {
        ControlledObject = GameObject.FindGameObjectWithTag("Player");
        CameraObject = GameObject.FindGameObjectWithTag("MainCamera");
    }
}
