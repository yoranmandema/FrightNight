using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SpriteShadow : MonoBehaviour {

	void Start () {
        EnableShadows();
    }

    void OnValidate() {
        EnableShadows();
    }

    void EnableShadows () {
        var renderer = GetComponent<Renderer>();

        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        renderer.receiveShadows = true;
    }
}
