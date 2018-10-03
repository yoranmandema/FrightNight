using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clown : MonoBehaviour {

    public GameObject TransformsTo;
    public Interactable Interactable;

    private void Start()
    {
        Interactable = GetComponent<Interactable>();
        Interactable.OnInteract += Interact;
    }

    public void Interact ()
    {
        var newClown = Instantiate(TransformsTo, transform.position, transform.rotation);

        print("interacted with clown");

        Destroy(gameObject);
    }
}
