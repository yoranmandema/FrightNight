using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBehaviour : MonoBehaviour {
    public Animator StateMachine;

    private void Start()
    {
        StateMachine = GetComponent<Animator>();
    }
}
