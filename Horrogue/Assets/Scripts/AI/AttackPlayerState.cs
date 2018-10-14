using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class AttackPlayerState : StateMachineBehaviour {
    public Attack attack;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        var attackSettings = animator.gameObject.GetComponent<AttackManager>();

        attackSettings.StartAttack(attack);
    }
}