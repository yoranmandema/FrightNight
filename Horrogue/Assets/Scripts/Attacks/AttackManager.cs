using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackManager : MonoBehaviour {
    #region Public Variables
    public AnimationController AnimationController;
    #endregion

    #region Private Variables
    private Animator stateMachine;
    #endregion

    void Start () {
        stateMachine = GetComponent<Animator>();
    }

    public void StartAttack(Attack attack) {
        attack.Initialise(gameObject);

        AnimationController.Play(attack.Name);

        StartCoroutine(DoAttack(attack));
    }

    IEnumerator DoAttack(Attack attack) {
        stateMachine.SetBool("Is Attacking", true);

        // Wait for when the attack happens in the animation.
        yield return new WaitForSeconds(attack.BeforeDelay);

        attack.TryDoDamage();

        // Let the animation finish.
        yield return new WaitForSeconds(attack.AfterDelay);

        stateMachine.SetBool("Is Attacking", false);

        AnimationController.Play("Idle");
    }
}
