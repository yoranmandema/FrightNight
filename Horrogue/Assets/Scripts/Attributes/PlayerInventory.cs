using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerInventory : MonoBehaviour {
    #region Public Variables
    public List<InventoryItem> Items;
    #endregion

    #region Private Variables
    private GUIStyle style;
    private GameManager gameManager;
    #endregion

    void Start () {
        style = new GUIStyle();
        style.fontSize = 30;
        style.normal.textColor = Color.red / 1.5f;

        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    void OnGUI() {
        if (gameManager.PlayerIsAlive) {
            var sorted = Items.OrderBy(o => o.Item.GetType().Name).ToList();

            for (int i = 0; i < sorted.Count; i++) {
                var item = sorted[i];

                GUI.Label(new Rect(50, 50 + i * 45, 400, 150), "[" + (i + 1) + "] " + item.Name + ": " + item.Amount, style);
            }           
        }
    }

    public void AddItemsOfType (GameObject itemObject,int amount) {
        bool existsInInventory = false;

        for (int i = 0; i < Items.Count; i++) {
            var item = Items[i];

            if (item.Item == itemObject) {
                item.Amount += amount;
                existsInInventory = true;

                if (item.Amount == 0) {
                    Items.RemoveAt(i);
                } 
                break;
            }
        }

        if (!existsInInventory && amount > 0) {
            Items.Add(new InventoryItem { Item = itemObject, Amount = amount });
        }
    }
}
