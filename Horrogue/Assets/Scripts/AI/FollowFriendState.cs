using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class FollowFriendState : StateMachineBehaviour {
    #region Private Variables
    private AIDestinationSetter destinationSetter;
    private GameObject friend;
    #endregion

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        destinationSetter = animator.gameObject.GetComponent<AIDestinationSetter>();

        friend = animator.gameObject.GetComponent<NPC>().ClosestFriend;
        friend.GetComponent<HeadReference>().Head.GetComponent<HeadAnimator>().State = HeadAnimator.AnimationState.Scared;

        animator.SetBool("Is Following Friend", true);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.SetBool("Is Following Friend", false);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        destinationSetter.target = friend.transform;
    }
}