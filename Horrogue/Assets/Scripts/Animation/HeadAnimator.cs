using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadAnimator : MonoBehaviour {
    public enum AnimationState {
        Idle,
        Scared,
        Kidnapped
    }

    #region Public Variables 
    public AnimationState State;
    #endregion

    #region Private Variables
    private DirectionObserver directionObserver;
    private AnimationController animationController;
    #endregion

    void Start() {
        directionObserver = GetComponent<DirectionObserver>();
        animationController = GetComponent<AnimationController>();
    }

    void Update() {
        switch (State) {
            case AnimationState.Idle:
                animationController.Play("Idle");
                break;
            case AnimationState.Scared:
                animationController.Play("Scared");
                StartCoroutine(ResetScared());
                break;
            case AnimationState.Kidnapped:
                animationController.Play("Kidnapped");
                break;
        }
    }

    IEnumerator ResetScared () {
        yield return new WaitForSeconds(1f);

        if (State == AnimationState.Scared) {
            State = AnimationState.Idle;
        }
    }
}
