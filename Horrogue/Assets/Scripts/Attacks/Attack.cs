using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Attack {
    public float Damage;
    public float BeforeDelay;
    public float AfterDelay;

    [HideInInspector]
    public GameObject Parent;
    private Animator stateMachine;

    public void Initialise (GameObject parent) {
        Parent = parent;
        stateMachine = Parent.GetComponent<Animator>();
    }

    public virtual void TryDoDamage () {
        var target = GameObject.FindObjectOfType<PlayerHealth>();
        var raycast = Physics2D.Linecast(Parent.transform.position, target.transform.position, ~LayerMask.GetMask("Enemy"));

        if (raycast.collider != null) {
            if (raycast.collider.gameObject == target.gameObject) {
                target.Value -= Damage;
            }
        }
    }
}
