using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clown : MonoBehaviour {

    public GameObject TransformsTo;

	public void Interact ()
    {
        var newClown = Instantiate(TransformsTo, transform.position, transform.rotation);

        Destroy(gameObject);
    }
}
