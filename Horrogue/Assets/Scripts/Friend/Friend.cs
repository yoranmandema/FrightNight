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
    public FriendStatus Status = FriendStatus.Kidnapped;

    private Interactable interactable;
    private AIPath aiPath;
    private GameManager gameManager;

    void Start () {
        aiPath = GetComponent<AIPath>();
        aiPath.canMove = false;

        interactable = GetComponent<Interactable>();
        interactable.OnInteract += OnInteract;

        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }
	
    public void OnGetKidnapped () {
        GetComponent<NPC>().StateMachine.Play("Following Clown");
    }

    public void OnKidnapped() {
        GetComponent<NPC>().StateMachine.Play("Idle");

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
