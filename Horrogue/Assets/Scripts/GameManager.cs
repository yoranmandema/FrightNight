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

    //TODO Children

	// Reference to layout generator
	public GameObject ControlledObject;
	public GameObject CameraObject;
    #endregion

    #region Private Variables
    private LayoutGenerator generator;
    private GameObject player;
    private GameObject clown;
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

        player = ControlledObject = (GameObject) Instantiate(playerPrefab, playerSpawn, Quaternion.identity);
        clown = (GameObject) Instantiate(clownPrefab, clownSpawn, Quaternion.identity);
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
