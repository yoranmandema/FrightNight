using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGiver : MonoBehaviour {

    #region Public Variables
    public GameObject Item;
    #endregion

    #region Public Variables
    private Interactable interactable;
    private Interact interact;
    private PlayerInventory playerInventory;
    private GameManager gameManager;
    private bool playerIsLooking;
    private GUIStyle style;
    #endregion

    void Start () {
        style = new GUIStyle {
            fontSize = 25,
            alignment = TextAnchor.MiddleCenter
        };
        style.normal.textColor = Color.red / 1.5f;

        interactable = GetComponent<Interactable>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        interactable.OnInteract += GiveItem;
    }

    void Update () {
        if (interact == null) {
            interact = gameManager.Player.GetComponent<Interact>();
        } else {
            if (interact.Hit.collider != null) {
                playerIsLooking = interact.Hit.collider.gameObject == gameObject;
            } else {
                playerIsLooking = false;
            }
        }
    }

    void GiveItem () {
        if (playerInventory == null) playerInventory = gameManager.Player.GetComponent<PlayerInventory>();

        playerInventory.AddItemsOfType(Item,1);
    }

    void OnGUI () {
        if (playerIsLooking) {
            GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 75, 400, 150), "PRESS [E] TO GET " + Item.GetComponent<Name>().Value.ToUpper() + ".", style);
        }
    }
}
