using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class LookForPlayerState : StateMachineBehaviour {
    [Tooltip("Determins how far the enemy can sense the player.")]
    [SerializeField] private float _lookRadius = 5.0f;

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        var player = GameObject.FindGameObjectWithTag("Player");
        var gameObject = animator.gameObject;

        RaycastHit2D rayResult = Physics2D.Raycast(
                gameObject.transform.position, 
                player.transform.position - gameObject.transform.position, 
                _lookRadius 
            );

        if (rayResult.collider != null) {
            animator.SetBool("Can See Player", rayResult.collider.gameObject == player);
        } else {
            animator.SetBool("Can See Player", false);
        }
    }
}