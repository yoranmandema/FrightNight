using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;

public class GameManager : MonoBehaviour {

	#region Public Variables
	[Header("Debugging")]
	[Tooltip("For debugging puposes")]
	public bool disableAutomaticGeneration = false;
	public KeyCode generationHotkey = KeyCode.G;

	// Player and NPC Prefabs
	[Header("Player/NPC Prefabs")]
    public GameObject playerPrefab;
    public GameObject clownPrefab;
    public GameObject friendPrefab;

	//TODO Children
	// References
	[Header("Important References")]
    public GameObject ControlledObject;
	public GameObject CameraObject;
	public GameObject Exit;

    [HideInInspector] public GameObject Player;
    [HideInInspector] public GameObject Clown;
    [HideInInspector] public List<GameObject> Friends = new List<GameObject>();
    [HideInInspector] public bool PlayerIsAlive;
	#endregion

	#region Private Variables
	private LayoutGenerator generator;
    #endregion

    // Use this for initialization
    void Start () {
		ControlledObject = GameObject.FindGameObjectWithTag("Player");
		CameraObject = GameObject.FindGameObjectWithTag("MainCamera");

        generator = GetComponent<LayoutGenerator>();

		if (!disableAutomaticGeneration)
		{
			GenerateLayout();
			SpawnCharacters();
		}
	}

    private void SpawnCharacters()
    {
        if (playerPrefab != null) {
            Vector3 playerSpawn = generator.GetPlayerSpawnPoint();
            Destroy(Player);
            Player = ControlledObject = Instantiate(playerPrefab, playerSpawn, Quaternion.identity);
        }

        if (clownPrefab != null) {
            Vector3 clownSpawn = generator.GetRandomSpawnPoint();
            Destroy(Clown);
            Clown = Instantiate(clownPrefab, clownSpawn, Quaternion.identity);
        }

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
		if (Input.GetKeyDown(generationHotkey))
		{
            GenerateLayout();
            SpawnCharacters();
		}
	}

    void GenerateLayout()
    {
        // Generate Layout
        if (generator != null) generator.GenerateLayout();

		Exit = generator.GetExit();

		// Scan Generated Layout
		AstarPath.active.Scan();
    }
}
