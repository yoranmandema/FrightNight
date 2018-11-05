using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyAnimator : MonoBehaviour {

    #region Private Variables
    private DirectionObserver directionObserver;
    private AnimationController animationController;
    #endregion

    void Start () {
        directionObserver = GetComponent<DirectionObserver>();
        animationController = GetComponent<AnimationController>();
    }
	
	void Update () {
        if (directionObserver.PositionDelta.magnitude > 0.25f) {
            animationController.Play("Walk");
        }
        else {
            animationController.Play("Idle");
        }
    }
}
