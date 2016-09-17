using UnityEngine;
using System.Collections;

public class FollowPlayer : MonoBehaviour {
    public GameObject Player;
    public float Offset = 4f;

	void FixedUpdate () {
        transform.position = new Vector3(transform.position.x,
                                         Player.transform.position.y + Offset,
                                         transform.position.z);
	}
}
