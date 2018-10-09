using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	#region Public Variables
	// Reference to layout generator
	public LayoutGenerator generator;

	#endregion

	#region Private Variables



	#endregion

	// Use this for initialization
	void Start () {
		if (generator == null)
		{
			generator = GetComponent<LayoutGenerator>();
		}

		generator.GenerateLayout();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
