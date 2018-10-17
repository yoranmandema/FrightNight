using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteOrder : MonoBehaviour {

    private Transform parentTransform;

    void Start () {
        parentTransform = transform.parent;

        SetZ();
    }

	void Update () {
		if (parentTransform.hasChanged) {
            SetZ();

            parentTransform.hasChanged = false;
        }
	}

    void SetZ() {
        var currentPos = transform.localPosition;
        currentPos.z = parentTransform.position.y / 50 + 10;

        transform.localPosition = currentPos;
    }
}
