using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firecracker : MonoBehaviour {

    #region Public Variables
    public float FuseTime = 2f;
    public GameObject SpriteObject;
    public float FlashTime = 5f;
    public float FlashRadius = 5f;
    #endregion

    #region Private Variables
    private GameManager gameManager;
    private LOS.LOSRadialLight effectLight;
    private float lt = 0;
    #endregion

    void Start () {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        effectLight = GetComponent<LOS.LOSRadialLight>();

        Invoke("Explode",FuseTime);
    }
	
    void Explode () {
        var clown = gameManager.Clown;
        var npc = clown.GetComponent<NPC>();

        npc.HearSound(transform.position);

        Destroy(SpriteObject);

        StartCoroutine(Effect());
    }

    IEnumerator Effect () {
        lt = FlashTime;

        while (true) {
            effectLight.radius = (lt / FlashTime) * FlashRadius;

            lt -= Time.deltaTime;

            if (lt < 0) {
                Destroy(gameObject);
            }

            yield return null;
        }
    }
}
