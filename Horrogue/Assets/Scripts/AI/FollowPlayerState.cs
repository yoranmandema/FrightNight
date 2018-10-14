using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class FollowPlayerState : StateMachineBehaviour {
    private AIDestinationSetter destinationSetter;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        destinationSetter = animator.gameObject.GetComponent<AIDestinationSetter>();

        animator.SetBool("Is Following Friend", true);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        destinationSetter.target = null;

        animator.SetBool("Is Following Friend", false);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        var player = GameObject.FindGameObjectWithTag("Player");

        destinationSetter.target = player.transform;
    }
}