using UnityEngine;
using System.Collections;

public class MoveTargetController : MonoBehaviour {
    public GameObject target;

    private Vector3 offset;

    public void Setup(GameObject target) {
        this.target = target;
        offset = transform.position - target.transform.position;
    }

    void FixedUpdate() {
        transform.position = target.transform.position + offset;
    }
}
