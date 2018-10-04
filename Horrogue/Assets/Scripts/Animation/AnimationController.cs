using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour {

    public string Direction = "Left";
    private Animator animator;

    public void Play (string type) {
        if (animator == null) return;

        animator.Play(type + " " + Direction, 0);
    }

	void Start () {
        animator = GetComponent<Animator>();

        Play("Idle");
    }
}
