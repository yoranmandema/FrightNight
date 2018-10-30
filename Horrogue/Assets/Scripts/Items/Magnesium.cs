using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magnesium : MonoBehaviour {

    #region Public Variables
    public float FuseTime = 2f;
    public float EffectiveDistance = 4f;
    public float EffectiveTime = 5f;
    public float BlindTime = 5f;
    #endregion

    #region Private Variables
    private GameManager gameManager;
    private bool isBurning;
    #endregion

    void Start() {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        Invoke("StartBurn", FuseTime);
    }

    void Update () {
        if (isBurning) {
            TryToBlindClown();
        }
    }

    void StartBurn () {
        isBurning = true;

        var light = GetComponent<LOS.LOSRadialLight>();
        light.radius = EffectiveDistance * 1.1f;

        Invoke("EndBurn", EffectiveTime);
    }

    void EndBurn() {
        Destroy(gameObject);
    }

    void TryToBlindClown() {
        var clown = gameManager.Clown;

        var rayCast = Physics2D.Raycast(transform.position, (clown.transform.position - transform.position).normalized, EffectiveDistance, LayerMask.GetMask(new[] { "Enemy", "Wall" }));

        if (rayCast.collider == null) return;

        if (rayCast.collider.gameObject == clown) {
            var npc = clown.GetComponent<NPC>();

            npc.FleeForSeconds(transform.position, BlindTime);
        }
    }
}
