using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalMotionSet : StateMachineBehaviour {

    public Motion Up;
    public Motion Down;
    public Motion Left;
    public Motion Right;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        switch ((int)animator.GetFloat("Direction")) {
            case 0:
                //AnimatorSt
                break;
            case 1:
                break;
            case 2:
                break;
            case 3:
                break;
        }
    }
}
