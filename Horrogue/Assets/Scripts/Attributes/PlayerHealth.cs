﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : CharacterAttribute {
    public bool IsAlive = true;
    public GUIStyle style;

    private float timeOfDeath;

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

        IsAlive = false;

        timeOfDeath = Time.realtimeSinceStartup;
    }

    void OnGUI () {
        GUI.Label(new Rect(50, Screen.height - 100, 400, 150), "HEALTH: " + _value, style);

        if (!IsAlive) {
            GUIDrawRect(new Rect(0, 0, Screen.width, Screen.height), new Color(0,0,0,(Time.realtimeSinceStartup - timeOfDeath)/3));

            GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 75, 400, 150), "YOU ARE DEAD", style);
        }
    }

    private static Texture2D _staticRectTexture;
    private static GUIStyle _staticRectStyle;

    public static void GUIDrawRect(Rect position, Color color) {
        if (_staticRectTexture == null) {
            _staticRectTexture = new Texture2D(1, 1);
        }

        if (_staticRectStyle == null) {
            _staticRectStyle = new GUIStyle();
        }

        _staticRectTexture.SetPixel(0, 0, color);
        _staticRectTexture.Apply();

        _staticRectStyle.normal.background = _staticRectTexture;

        GUI.Box(position, GUIContent.none, _staticRectStyle);
    }
}