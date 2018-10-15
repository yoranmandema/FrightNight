using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteOrder : MonoBehaviour {

    private Transform parentTransform;

    void Start () {
        parentTransform = transform.parent;
        transform.localPosition = new Vector3(0, 0, (parentTransform.position.y / 50 + 10));
    }

	void Update () {
		if (parentTransform.hasChanged) {
            transform.localPosition = new Vector3(0,0,(parentTransform.position.y/50 + 10));

            parentTransform.hasChanged = false;
        }
	}
}
