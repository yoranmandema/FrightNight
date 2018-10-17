using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class CheckSoundState : StateMachineBehaviour {
    #region Private Variables
    private AIDestinationSetter destinationSetter;
    private AIPath aiPath;

    private GameManager gameManager;
    private LayoutGenerator layoutManager;
    private NPC npc;

    private GameObject gameObject;
    private Vector3 targetLocation;
    #endregion

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        gameObject = animator.gameObject;

        destinationSetter = gameObject.GetComponent<AIDestinationSetter>();
        aiPath = gameObject.GetComponent<AIPath>();
        npc = gameObject.GetComponent<NPC>();

        var gm = GameObject.FindGameObjectWithTag("GameManager");
        gameManager = gm.GetComponent<GameManager>();
        layoutManager = gm.GetComponent<LayoutGenerator>();

        // Set Location
        targetLocation = GetRandomNode().position;
        Debug.Log(npc.DestinationSlave.transform.position);
        destinationSetter.target = npc.DestinationSlave.transform;

        animator.SetBool("Reached Destination", false);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        npc.DestinationSlave.transform.position = targetLocation;
        destinationSetter.target = npc.DestinationSlave.transform;

        if (Vector2.Distance(gameObject.transform.position, targetLocation) < 1) {
            OnArrived(animator);
        }
    }

    void OnArrived(Animator animator) {
        destinationSetter.target = null;

        animator.SetBool("Reached Destination", true);
    }

    NNInfo GetRandomNode() {
        var graph = AstarPath.active.data.gridGraph;

        var distance = Vector2.Distance(npc.LastHeardSound, gameObject.transform.position);

        return AstarPath.active.GetNearest((Vector2)npc.LastHeardSound + new Vector2(distance, distance) * Random.Range(-1f, 1f) * 0.5f);
    }
}