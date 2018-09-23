using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : CharacterAttribute {
    public bool IsAlive = true;
    public GUIStyle style;

    public override void Initialise() {
        style = new GUIStyle();
        style.fontSize = 50;
        style.normal.textColor = Color.red / 1.5f;
    }

    public override void OnValueDepleted() {
        base.OnValueDepleted();

        var enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (var enemy in enemies) {
            var stateMachine = enemy.GetComponent<Animator>();

            if (stateMachine != null) {
                stateMachine.SetBool("Player Is Alive", false);
            } 
        }

        var movement = GetComponent<PlayerMovement>();
        movement.enabled = false;

        var aim = GetComponent<LightAim>();
        aim.enabled = false;

        IsAlive = false;
    }

    void OnGUI () {
        GUI.Label(new Rect(50, Screen.height - 100, 400, 150), "HEALTH: " + _value, style);

        if (!IsAlive) {
            GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 75, 400, 150), "YOU ARE DEAD", style);
        }
    }
}
