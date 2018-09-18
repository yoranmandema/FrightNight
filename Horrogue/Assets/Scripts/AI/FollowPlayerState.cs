using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class FollowPlayerState : StateMachineBehaviour {
    private AIDestinationSetter destinationSetter;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        destinationSetter = animator.gameObject.GetComponent<AIDestinationSetter>();

        animator.SetBool("Is Following Player", true);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        destinationSetter.target = null;

        animator.SetBool("Is Following Player", false);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        var player = GameObject.Find("Player");

        destinationSetter.target = player.transform;
    }
}