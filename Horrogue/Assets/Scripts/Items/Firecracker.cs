using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firecracker : MonoBehaviour {

    #region Public Variables
    public float FuseTime = 2f;
    #endregion

    #region Private Variables
    private GameManager gameManager;
    #endregion

    void Start () {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        Invoke("Explode",FuseTime);
    }
	
    void Explode () {
        var clown = gameManager.Clown;
        var npc = clown.GetComponent<NPC>();

        npc.HearSound(transform.position);

        Destroy(gameObject);
    }
}
