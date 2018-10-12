using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationSlave : MonoBehaviour {
    public Vector3 Position { get; private set; }

    public void SetTargetPosition (Vector3 target) {
        Position = target;
    }

    void Start () {
        Position = transform.position;
    }

    void Update () {
        transform.position = Position;
    }
}
