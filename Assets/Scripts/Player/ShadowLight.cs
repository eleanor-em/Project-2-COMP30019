using UnityEngine;
using System.Collections;

public class ShadowLight : MonoBehaviour {
    public GameObject player;
    private Vector3 offset;

    void Start() {
        offset = transform.position - player.transform.position;
    }

    void FixedUpdate() {
        transform.position = player.transform.position + offset;
    }
}
