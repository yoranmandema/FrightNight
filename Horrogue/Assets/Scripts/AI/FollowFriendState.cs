using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class FollowFriendState : StateMachineBehaviour {
    private AIDestinationSetter destinationSetter;
    private AIPath aiPath;
    private GameObject friend;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        destinationSetter = animator.gameObject.GetComponent<AIDestinationSetter>();
        aiPath = animator.gameObject.GetComponent<AIPath>();

        friend = animator.gameObject.GetComponent<NPC>().ClosestFriend;

        animator.SetBool("Is Following Friend", true);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.SetBool("Is Following Friend", false);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        destinationSetter.target = friend.transform;
    }
}