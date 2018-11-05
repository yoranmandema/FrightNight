using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(AIPath)), RequireComponent(typeof(AIDestinationSetter)), RequireComponent(typeof(Animator))]
public class NPC : AIBehaviour {
    [Tooltip("Determins how far the enemy can sense the player.")]
    [SerializeField] private float _lookRadius = 5.0f;

    #region Public Variables
    //public AnimationController AnimationController;
    public GameObject ClosestFriend;
    public GameObject DestinationSlave;
    public Vector3 LastHeardSound;
    public Vector3 FleePosition;
    #endregion

    #region Private Variables
    private GameManager gameManager;
    #endregion

    void Start () {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        //if (AnimationController != null) AnimationController.Play("Idle");
    }

    void Update() {
        LookForPlayer();
        LookForFriends();
        //SetDirection();

        StateMachine.SetBool("Player is being followed by friends", gameManager.PlayerFollowedByFriends());
        StateMachine.SetBool("Player Is Alive", gameManager.PlayerIsAlive);
        //StateMachine.SetBool("Heard Sound", false);
    }

    //void SetDirection() {
    //    var direction = transform.rotation * Vector3.up;

    //    if (direction.x > 0 && AnimationController != null) {
    //        AnimationController.Direction = "Right";
    //    }
    //    else {
    //        AnimationController.Direction = "Left";
    //    }
    //}

    void LookForFriends() {
        var friends = GameObject.FindGameObjectsWithTag("Friend");

        StateMachine.SetBool("Can See Friend", false);
        StateMachine.SetFloat("Distance To Closest Friend", Mathf.Infinity);

        foreach (var friend in friends) {
            if (friend != null) {
                if (friend.GetComponent<Friend>().Status != FriendStatus.FollowingPlayer) continue;

                // Check wether something is between the player and the AI
                RaycastHit2D rayResult = Physics2D.Raycast(
                        gameObject.transform.position,
                        friend.transform.position - gameObject.transform.position,
                        _lookRadius,
                        ~LayerMask.GetMask(new[] { "Sprite", "Floor", "Enemy" })
                    );

                var distance = Vector2.Distance(friend.transform.position, gameObject.transform.position);

                if (distance < StateMachine.GetFloat("Distance To Closest Friend")) {
                    StateMachine.SetFloat("Distance To Closest Friend", distance);

                    ClosestFriend = friend;
                }

                if (rayResult.collider != null && rayResult.collider.gameObject == friend) {
                    StateMachine.SetBool("Can See Friend", true);
                }
            }
        }
    }

    void LookForPlayer () {
        var player = GameObject.FindGameObjectWithTag("Player");

        if (player != null) {
            // Check wether something is between the player and the AI
            RaycastHit2D rayResult = Physics2D.Raycast(
                    gameObject.transform.position,
                    player.transform.position - gameObject.transform.position,
                    _lookRadius,
                    ~LayerMask.GetMask(new[] { "Sprite", "Floor", "Enemy", "Friend" })
                );

            StateMachine.SetFloat("Distance To Player", Vector2.Distance(player.transform.position, gameObject.transform.position));

            if (rayResult.collider != null) {
                StateMachine.SetBool("Can See Player", rayResult.collider.gameObject == player);
            }
            else {
                StateMachine.SetBool("Can See Player", false);
            }
        }
    }

    public void HearSound (Vector3 position) {
        StateMachine.SetBool("Heard Sound", true);
    
        LastHeardSound = position;

        StartCoroutine(resetSoundState());
    }

    IEnumerator resetSoundState() {
        yield return new WaitForSeconds(0.1f);

        StateMachine.SetBool("Heard Sound", false);
    }

    public void FleeForSeconds (Vector3 position, float time) {
        StateMachine.Play("Flee", 0);
        FleePosition = position;

        StartCoroutine(resetFleeState(time));
    }

    IEnumerator resetFleeState(float time) {
        yield return new WaitForSeconds(time);

        StateMachine.Play("Idle",0);
    }
}
