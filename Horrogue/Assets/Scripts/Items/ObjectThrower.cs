using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectThrower : MonoBehaviour {
    #region Public Variables
    public LightAim Aim;
    #endregion

    #region Private Variables
    private PlayerInventory inventory;
    #endregion

    void Start () {
        Aim = GetComponent<LightAim>();
        inventory = GetComponent<PlayerInventory>();
    }

	void Update () {
        for (int i = 1; i < 10; ++i) {
            if (i > inventory.Items.Count) break;

            if (Input.GetKeyDown("" + i)) {
                var item = inventory.Items[i-1].Item;

                inventory.AddItemsOfType(item, -1);

                var throwedObject = Instantiate(item, transform.position, Quaternion.Euler(0, 0, 0));
                throwedObject.GetComponent<Throwable>().Throw(Quaternion.Euler(0, 0, Aim.Rotation) * Vector2.right);
            }
        }
    }
}
