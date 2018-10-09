using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public enum FriendStatus {
    Kidnapped,
    Following
}

public class FriendInteractable : MonoBehaviour {
    public FriendStatus Status = FriendStatus.Kidnapped;

    private Interactable interactable;
    private AIPath aiPath;

    void Start () {
        aiPath = GetComponent<AIPath>();
        aiPath.canMove = false;

        interactable = GetComponent<Interactable>();
        interactable.OnInteract += OnInteract;
    }
	
	void OnInteract () {
		switch (Status) {
            case FriendStatus.Kidnapped:
                Status = FriendStatus.Following;

                aiPath.canMove = true;
                break;
        }
	}
}
