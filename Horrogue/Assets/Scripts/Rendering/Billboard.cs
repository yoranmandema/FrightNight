using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Billboard : MonoBehaviour {
    public Texture Diffuse;
    public Texture Normal;

    void OnValidate() {
        var renderer = GetComponent<Renderer>();
        var material = renderer.sharedMaterial;

        material.SetTexture("_MainTex",Diffuse);
        material.SetTexture("_BumpMap", Normal);

        LookAtCamera();
    }

    void Update () {
        LookAtCamera();
    }

    void LookAtCamera () {
        var camPos = Camera.main.transform.position;
        var ownPos = transform.position;
        var direction = (camPos - ownPos).normalized;

        transform.rotation = Quaternion.LookRotation(-direction);
    }
}
