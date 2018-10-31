using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[System.Serializable]
public class InventoryItem {
    public GameObject Item;
    public int Amount;
    public string Name { get { return Item.GetComponent<Name>().Value; } }
}