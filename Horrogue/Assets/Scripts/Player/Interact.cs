using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interact : MonoBehaviour {
    public float InteractDistance = 0.5f;

	void Update () {
		if (Input.GetButtonDown("Interact"))
        {
            var cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var direction = (cursorPos - transform.position).normalized;

            var hit = Physics2D.CircleCast(transform.position, 0.2f, direction, InteractDistance, ~LayerMask.GetMask("Player"));

            if (hit.collider != null)
            {
                var component = hit.collider.gameObject.GetComponent<Interactable>();

                if (component != null)
                {
                    component.OnInteract();
                }
            }
        }
	}
}
