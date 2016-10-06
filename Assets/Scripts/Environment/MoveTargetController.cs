using UnityEngine;
using System.Collections;

public class MoveTargetController : MonoBehaviour {
    private GameObject target;
    new private Light light;
    private Vector3 offset;

    public void Setup(GameObject target) {
        this.target = target;
        offset = transform.position - target.transform.position;
        light = gameObject.AddComponent<Light>();
        light.transform.position = transform.position;
        light.color = Color.white;
    }

    void FixedUpdate() {
        transform.position = target.transform.position + offset;
        light.transform.position = transform.position;
    }
}
