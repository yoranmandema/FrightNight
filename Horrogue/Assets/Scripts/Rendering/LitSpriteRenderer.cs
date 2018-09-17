using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LitSpriteRenderer : MonoBehaviour {
    [SerializeField]
    private Texture spriteTexture;
    [SerializeField]
    private Texture normalTexture;
    [SerializeField]
    private Color color = Color.white;

    void OnValidate() {
        Renderer renderer = GetComponent<Renderer>();

        renderer.material.EnableKeyword("_NORMALMAP");

        renderer.material.color = color;
        renderer.material.SetTexture("_MainTex", spriteTexture);
        renderer.material.SetTexture("_BumpMap", normalTexture);
    }
}
