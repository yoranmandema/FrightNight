using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSettings : MonoBehaviour {
    public Attack[] Attacks;

    private Animator stateMachine;

    void Start () {
        foreach (var attack in Attacks) {
            attack.Initialise(gameObject);
        }

        stateMachine = GetComponent<Animator>();
    }

    public void StartAttack() {
        var attack = Attacks[0];

        stateMachine.SetBool("Is Attacking", true);
        stateMachine.SetBool("Has Finished Attack", false);
        StartCoroutine(DoAttack(attack));
    }

    IEnumerator DoAttack(Attack attack) {
        yield return new WaitForSeconds(attack.BeforeDelay);
        attack.TryDoDamage();
        yield return new WaitForSeconds(attack.AfterDelay);
        stateMachine.SetBool("Is Attacking", false);
        stateMachine.SetBool("Has Finished Attack", true);
    }
}
