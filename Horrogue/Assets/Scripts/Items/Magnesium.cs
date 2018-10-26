using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magnesium : MonoBehaviour {

    #region Public Variables
    public float FuseTime = 2f;
    public float EffectiveDistance = 4f;
    public float EffectiveTime = 5f;
    #endregion

    #region Private Variables
    private GameManager gameManager;
    #endregion

    void Start() {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        Invoke("TryToBlindClown", FuseTime);
    }

    void TryToBlindClown() {
        var clown = gameManager.Clown;
        var rayCast = Physics2D.Raycast(transform.position, (clown.transform.position - transform.position).normalized, EffectiveDistance, LayerMask.GetMask(new[] { "Enemy", "Wall" }));

        print(rayCast.collider);

        if (rayCast.collider == null) {
            Destroy(gameObject);
            return;
        }

        if (rayCast.collider.gameObject == clown) {
            var npc = clown.GetComponent<NPC>();

            npc.FleeForSeconds(transform.position, EffectiveTime);
        }

        Destroy(gameObject);
    }
}
