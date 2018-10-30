using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class KidnapFriendState : StateMachineBehaviour {
    #region Private Variables
    private AIDestinationSetter destinationSetter;
    private AIPath aiPath;

    private GameManager gameManager;
    private LayoutGenerator layoutManager;
    private NPC npc;
    private Clown clown;

    private GameObject friend;
    private GameObject gameObject;
    #endregion

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        gameObject = animator.gameObject;

        destinationSetter = gameObject.GetComponent<AIDestinationSetter>();
        aiPath = gameObject.GetComponent<AIPath>();
        npc = gameObject.GetComponent<NPC>();
        clown = gameObject.GetComponent<Clown>();

        var gm = GameObject.FindGameObjectWithTag("GameManager");
        gameManager = gm.GetComponent<GameManager>();
        layoutManager = gm.GetComponent<LayoutGenerator>();

        // Set Location
        npc.DestinationSlave.transform.position = layoutManager.GetRandomSpawnPoint();
        destinationSetter.target = npc.DestinationSlave.transform;

        // Make friend follow clown
        friend = npc.ClosestFriend;
        friend.GetComponent<Friend>().OnGetKidnapped();

        clown.KidnappedFriend = friend;

        animator.SetBool("Is Kidnapping", true);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        destinationSetter.target = npc.DestinationSlave.transform;

        if (Vector2.Distance(gameObject.transform.position, npc.DestinationSlave.transform.position) < 1) {
            OnArrived(animator);
        }
    }

    void OnArrived(Animator animator) {
        destinationSetter.target = null;

        friend.GetComponent<Friend>().OnKidnapped();
        clown.KidnappedFriend = null;

        animator.SetBool("Is Kidnapping", false);
    }
}