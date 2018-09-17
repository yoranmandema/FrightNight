using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerDistanceSound : MonoBehaviour {
    AudioSource audioData;
    public Transform target;

    void Start() {
        audioData = GetComponent<AudioSource>();
        audioData.Play(0);
    }

    void Update() {
        audioData.volume = Mathf.Clamp(Mathf.Sin(((3 - Vector3.Distance(transform.position, target.position)) / 3) * Mathf.PI / 4),0,1);
    }
}
