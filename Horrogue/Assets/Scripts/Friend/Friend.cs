using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public enum FriendStatus {
    Kidnapped,
    FollowingPlayer,
    FollowingKiller,  
}

public class Friend : MonoBehaviour {
    #region Public Variables
    public FriendStatus Status = FriendStatus.Kidnapped;
    #endregion

    #region Private Variables
    private Interactable interactable;
    private AIPath aiPath;
    private GameManager gameManager;
    private HeadAnimator headAnimator;
    #endregion

    void Start () {
        aiPath = GetComponent<AIPath>();
        aiPath.canMove = false;

        interactable = GetComponent<Interactable>();
        interactable.OnInteract += OnInteract;

        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        headAnimator = GetComponent<HeadReference>().Head.GetComponent<HeadAnimator>();
    }

    void Update () {
        if (headAnimator.State == HeadAnimator.AnimationState.Kidnapped && Status == FriendStatus.FollowingPlayer) {
            headAnimator.State = HeadAnimator.AnimationState.Idle;
        }
    }
	
    public void OnGetKidnapped () {
        GetComponent<NPC>().StateMachine.Play("Following Clown");
        headAnimator.State = HeadAnimator.AnimationState.Kidnapped;
        Status = FriendStatus.FollowingKiller;
    }

    public void OnKidnapped() {
        GetComponent<NPC>().StateMachine.Play("Idle");
        headAnimator.State = HeadAnimator.AnimationState.Idle;

        Status = FriendStatus.Kidnapped;
        aiPath.canMove = false;
    }

    void OnInteract () {
		switch (Status) {
            case FriendStatus.Kidnapped:
                Status = FriendStatus.FollowingPlayer;

                aiPath.canMove = true;
                break;
        }
	}
}
