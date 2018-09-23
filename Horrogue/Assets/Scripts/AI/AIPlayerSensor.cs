using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayerSensor : AIBehaviour {
    [Tooltip("Determins how far the enemy can sense the player.")]
    [SerializeField] private float _lookRadius = 5.0f;

    void Update()
    {
        var player = GameObject.FindGameObjectWithTag("Player");

        // Check wether something is between the player and the AI
        RaycastHit2D rayResult = Physics2D.Raycast(
                gameObject.transform.position,
                player.transform.position - gameObject.transform.position,
                _lookRadius,
                ~LayerMask.GetMask(new[] { "Sprite" , "Floor", "Enemy" })
            );

        StateMachine.SetFloat("Distance To Player", Vector2.Distance(player.transform.position, gameObject.transform.position));

        if (rayResult.collider != null)
        {
            StateMachine.SetBool("Can See Player", rayResult.collider.gameObject == player);
        }
        else
        {
            //pixelate.pixelSizeX = 0;
            StateMachine.SetBool("Can See Player", false);
        }
    }
}
