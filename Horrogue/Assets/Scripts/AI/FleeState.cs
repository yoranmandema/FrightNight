using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class FleeState : StateMachineBehaviour {
    #region Private Variables
    private AIDestinationSetter destinationSetter;
    private AIPath aiPath;

    private GameManager gameManager;
    private LayoutGenerator layoutManager;
    private NPC npc;
    private Clown clown;

    private GameObject gameObject;
    private Vector3 targetLocation;
    #endregion

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        // Get references.
        gameObject = animator.gameObject;

        destinationSetter = gameObject.GetComponent<AIDestinationSetter>();
        aiPath = gameObject.GetComponent<AIPath>();
        npc = gameObject.GetComponent<NPC>();
        clown = gameObject.GetComponent<Clown>();

        var gm = GameObject.FindGameObjectWithTag("GameManager");
        gameManager = gm.GetComponent<GameManager>();
        layoutManager = gm.GetComponent<LayoutGenerator>();

        // Reset Friend.
        var friend = clown.KidnappedFriend;

        if (friend != null) {
            friend.GetComponent<NPC>().StateMachine.Play("Lost", 0);
            friend.GetComponent<Friend>().Status = FriendStatus.FollowingPlayer;
            clown.KidnappedFriend = null;
        }

        // Set target location.
        targetLocation = GetRandomNode().position;
        destinationSetter.target = npc.DestinationSlave.transform;

        animator.SetBool("Reached Destination", false);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        npc.DestinationSlave.transform.position = targetLocation;
        destinationSetter.target = npc.DestinationSlave.transform;
    }

    NNInfo GetRandomNode() {
        // Get position in opposit direction of flee position.
        var goToPosition = (gameObject.transform.position - npc.FleePosition).normalized * 100f;

        return AstarPath.active.GetNearest(goToPosition);
    }
}