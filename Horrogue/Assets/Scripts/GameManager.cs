using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;

public class GameManager : MonoBehaviour {

    #region Public Variables
    // Player and NPC Prefabs
    public GameObject playerPrefab;
    public GameObject clownPrefab;
    public GameObject friendPrefab;

    //TODO Children

    // References
    public GameObject ControlledObject;
	public GameObject CameraObject;

    [HideInInspector] public GameObject Player;
    [HideInInspector] public GameObject Clown;
    [HideInInspector] public List<GameObject> Friends = new List<GameObject>();
    #endregion

    #region Private Variables
    private LayoutGenerator generator;
    #endregion

    // Use this for initialization
    void Start () {
		ControlledObject = GameObject.FindGameObjectWithTag("Player");
		CameraObject = GameObject.FindGameObjectWithTag("MainCamera");

        generator = GetComponent<LayoutGenerator>();

        GenerateLayout();
        SpawnCharacters();
	}

    private void SpawnCharacters()
    {
        Vector3 playerSpawn = generator.GetPlayerSpawnPoint();
        Vector3 clownSpawn = generator.GetRandomSpawnPoint();

        Destroy(Player);
        Destroy(Clown);

        Player = ControlledObject = Instantiate(playerPrefab, playerSpawn, Quaternion.identity);
        Clown = Instantiate(clownPrefab, clownSpawn, Quaternion.identity);

        SpawnFriends();
    }

    private void SpawnFriends () {
        foreach (var friend in Friends) {
            if (friend != null)
                Destroy(friend);
        }

        Friends.Clear();

        for (int i = 0; i < 3; i++) {
            Vector3 friendSpawn = generator.GetRandomSpawnPoint(true);
            var friend = Instantiate(friendPrefab, friendSpawn, Quaternion.identity);

            Friends.Add(friend);
        }
    }

    public bool PlayerFollowedByFriends () {
        bool isBeingFollowed = false;

        foreach (var friend in Friends) {
            if (friend.GetComponent<Friend>().Status == FriendStatus.FollowingPlayer) {
                isBeingFollowed = true;
            }
        }

        return isBeingFollowed;
    }

    private void Update()
	{
		if (Input.GetKeyDown(KeyCode.G))
		{
            GenerateLayout();
            SpawnCharacters();
		}
	}

    void GenerateLayout()
    {
        // Generate Layout
        generator.GenerateLayout();

        // Scan Generated Layout
        AstarPath.active.Scan();
    }
}
