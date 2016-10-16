using UnityEngine;
using System.Collections;

public class MovingPlatform : MonoBehaviour {
    public Vector3 Speed;

    void OnTriggerEnter(Collider collider) {
        if (collider.CompareTag("MovingPlatformMirror")) {
            Speed *= -1;
        }
    }

    void FixedUpdate() {
        transform.position += Speed * Time.fixedDeltaTime;
    }
}
