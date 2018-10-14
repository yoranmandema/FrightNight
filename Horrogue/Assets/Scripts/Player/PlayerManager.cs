using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {
    public List<GameObject> FollowingFriends;

    public bool IsBeingFollowedByFriends() {
        return FollowingFriends.Count > 0;
    }
}
