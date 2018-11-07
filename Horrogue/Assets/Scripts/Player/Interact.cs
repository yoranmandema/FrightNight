using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interact : MonoBehaviour {
    public float InteractDistance = 0.5f;
    public RaycastHit2D Hit;

	void Update () {
        var cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var direction = (cursorPos - transform.position).normalized;

        Hit = Physics2D.CircleCast(transform.position, 0.2f, direction, InteractDistance, ~LayerMask.GetMask("Player", "Default"));

        if (Input.GetButtonDown("Interact"))
        {
            if (Hit.collider != null)
            {
                var component = Hit.collider.gameObject.GetComponent<Interactable>();

                if (component != null)
                {
                    component.OnInteract();
                }
            }
        }
	}
}
