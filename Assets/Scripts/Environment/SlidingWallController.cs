using UnityEngine;
using System.Collections;

public class SlidingWallController : MonoBehaviour {
    public float Speed = 0.1f;
    public Vector3 slidingPosition;
    private Vector3 initialPosition;
    private int sliding = 0;

    void Start() {
        initialPosition = transform.position;
    }

    public void Move() {
        // If we didn't move to the bottom
        if (sliding == 0 || sliding == -1) {
            sliding = 1;
        } else {
            sliding = -1;    
        }
    }

    void FixedUpdate() {
        if (sliding != 0) {
            Vector3 target = (sliding < 0) ? (initialPosition) : (slidingPosition);
            Vector3 s = target - transform.position;
            if (s.sqrMagnitude > 0.001f) {
                transform.position += s * Speed * Time.fixedDeltaTime;
            }
        } 
    }
}
