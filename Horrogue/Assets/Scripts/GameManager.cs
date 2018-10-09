using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	#region Public Variables
	// Reference to layout generator
	public LayoutGenerator generator;
	public GameObject ControlledObject;
	public GameObject CameraObject;
	#endregion

	#region Private Variables



	#endregion

	// Use this for initialization
	void Start () {
		ControlledObject = GameObject.FindGameObjectWithTag("Player");
		CameraObject = GameObject.FindGameObjectWithTag("MainCamera");

		if (generator == null)
		{
			generator = GetComponent<LayoutGenerator>();
		}

		generator.GenerateLayout();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.G))
		{
			generator.GenerateLayout();
		}
	}
}
