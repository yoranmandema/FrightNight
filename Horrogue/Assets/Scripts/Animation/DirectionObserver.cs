using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionObserver : MonoBehaviour {

    public enum AnimationStyle {
        LeftRight,
        AllDirections
    }

    #region Public Variables
    public Transform Target;
    public AnimationStyle Style;
    public Vector3 PositionDelta;
    #endregion

    #region Private Variables
    private AnimationController animationController;
    private Vector3 lastPosition;
    #endregion

    void Start () {
        lastPosition = Target.position;
        animationController = GetComponent<AnimationController>();
    }

    void Update() {
        PositionDelta = (Target.position - lastPosition) / Time.deltaTime;
        lastPosition = Target.position;

        if (Style == AnimationStyle.LeftRight) {
            SetDirectionLeftRight();
        }
        else if (Style == AnimationStyle.AllDirections) {
            SetDirection();
        }
    }

    void SetDirectionLeftRight() {
        if (PositionDelta.x > 0) {
            animationController.Direction = "Right";
        }
        else {
            animationController.Direction = "Left";
        }
    }

    void SetDirection() {
        if (Mathf.Abs(PositionDelta.x) > Mathf.Abs(PositionDelta.y)) {
            if (PositionDelta.x > 0) {
                animationController.Direction = "Right";
            }
            else {
                animationController.Direction = "Left";
            }
        }
        else {
            if (PositionDelta.y > 0) {
                animationController.Direction = "Up";
            }
            else {
                animationController.Direction = "Down";
            }
        }
    }
}
