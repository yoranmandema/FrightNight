using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locker : MonoBehaviour {

    [HideInInspector]
    public bool IsInside = false;
    public GameObject Visibility;

    private Interact interact;
    private Interactable interactable;
    private GameObject player;
    private GameManager gameManager;
    
	void Start () {
        interactable = GetComponent<Interactable>();
        interact = GetComponent<Interact>();

        player = GameObject.FindGameObjectWithTag("Player");
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        Visibility.SetActive(false);
        interact.enabled = false;

        interactable.OnInteract += OnInteract;
    }
	
	void OnInteract () {
        IsInside = !IsInside;

        if (IsInside) {
            player.SetActive(false);
            Visibility.SetActive(true);

            gameManager.ControlledObject = gameObject;
            interact.enabled = true;
        } else {
            player.SetActive(true);
            Visibility.SetActive(false);

            gameManager.ControlledObject = player;
            interact.enabled = false;
        }
    }
}
