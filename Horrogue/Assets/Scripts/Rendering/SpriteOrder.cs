using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteOrder : MonoBehaviour {

    public float Offset = 0f; 
    private Transform parentTransform;

    void Start () {
        parentTransform = transform.parent;

        SetZ();
    }

    void Update() {
        SetZ();
	}

    void SetZ() {
        var currentPos = transform.localPosition;
        currentPos.z = (parentTransform.position.y + Offset) / 50 + 10;

        transform.localPosition = currentPos;
    }
}
