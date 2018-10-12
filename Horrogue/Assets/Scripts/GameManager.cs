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

    // Reference to layout generator
    public GameObject ControlledObject;
	public GameObject CameraObject;
    #endregion

    #region Private Variables
    private LayoutGenerator generator;
    private GameObject player;
    private GameObject clown;
    private List<GameObject> friends = new List<GameObject>();
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

        Destroy(player);
        Destroy(clown);

        foreach (var friend in friends) {
            if (friend != null)
                Destroy(friend);
        }

        friends.Clear();

        player = ControlledObject = Instantiate(playerPrefab, playerSpawn, Quaternion.identity);
        clown = Instantiate(clownPrefab, clownSpawn, Quaternion.identity);

        for (int i = 0; i < 3; i++) {
            Vector3 friendSpawn = generator.GetRandomSpawnPoint();
            var friend = Instantiate(friendPrefab, friendSpawn, Quaternion.identity);

            friends.Add(friend);
        }
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
