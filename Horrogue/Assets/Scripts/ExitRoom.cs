using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitRoom : MonoBehaviour {

    #region Private Variables
    private BoxCollider2D roomCollider;
    private bool playerIsInRoom;
    private GUIStyle style;
    private bool hasEscaped;
    private float escapeTime;
    private int amountOfFriendsSaved;
    private int amountOfFriendsDeserted;
    private GameManager gameManager;
    #endregion

    void Start () {
        style = new GUIStyle();
        style.fontSize = 25;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.red / 1.5f;

        roomCollider = GetComponent<BoxCollider2D>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown("Interact") && playerIsInRoom) {
            hasEscaped = true;
            escapeTime = Time.realtimeSinceStartup;

            var friends = GameObject.FindGameObjectsWithTag("Friend");

            foreach (var friend in friends) {
                if (friend.GetComponent<Friend>().Status == FriendStatus.FollowingPlayer) {
                    amountOfFriendsSaved++;
                }
            }

            amountOfFriendsDeserted = friends.Length - amountOfFriendsSaved;

            Destroy(gameManager.Clown);

            var movement = gameManager.Player.GetComponent<PlayerMovement>();
            movement.enabled = false;
        }
	}

    void OnGUI() {
        if (playerIsInRoom && !hasEscaped) {
            GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 75, 400, 150), "YOU REACHED THE EXIT. PRESS [E] TO ESCAPE.", style);
        }

        if (hasEscaped) {
            PlayerHealth.GUIDrawRect(new Rect(0, 0, Screen.width, Screen.height), new Color(0, 0, 0, (Time.realtimeSinceStartup - escapeTime) / 3));

            GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 150, 400, 200), "YOU ESCAPED THE SCHOOL.", style);
            GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 100, 400, 200), "YOU SAVED " + amountOfFriendsSaved + " OF YOUR FRIENDS...", style);
            GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 50, 400, 200), "...AND LEFT BEHIND " + amountOfFriendsDeserted + " OF THEM.", style);
        }
    }

    void OnCollisionEnter2D(Collision2D target) {
        if (target.gameObject.tag.Equals("Player")) {
            playerIsInRoom = true;
        }
    }


    void OnCollisionExit2D(Collision2D target) {
        if (target.gameObject.tag.Equals("Player")) {
            playerIsInRoom = false;
        }
    }


}
