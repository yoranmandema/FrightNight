using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class AttackPlayerState : StateMachineBehaviour {
    private AIDestinationSetter destinationSetter;
    private Attack attack;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        var attackSettings = animator.gameObject.GetComponent<AttackSettings>();

        attackSettings.StartAttack();
    }
}