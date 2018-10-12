using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class FollowClownState : StateMachineBehaviour {
    private Vector3 oldPos;
    private Vector3 addPos;
    private GameObject clown;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        clown = GameObject.FindGameObjectWithTag("Enemy");
        oldPos = clown.transform.position;

        animator.SetBool("Is Following Clown", true);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.SetBool("Is Following Clown", false);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        var newPos = clown.transform.position;

        addPos = Vector3.Lerp((newPos - oldPos).normalized * -0.5f, addPos, 0.25f);

        oldPos = newPos;

        animator.gameObject.transform.position = newPos + addPos;
    }
}