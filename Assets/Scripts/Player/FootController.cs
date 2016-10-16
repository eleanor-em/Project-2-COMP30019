using UnityEngine;

public class FootController : MonoBehaviour {
    public bool Moving { get; set; }
    public float amplitude;
    public float period;
    private float radius;
    private float t;
    private float z;

    public void Reset() {
        Moving = false;
        transform.localPosition = new Vector3(0, radius, z);
        t = 0;
    }

    void Start() {
        radius = transform.localPosition.y;
        z = transform.localPosition.z;
    }

    void FixedUpdate() {
        if (Moving) {
            // This is the solution to the differential equation describing a pendulum
            float theta = amplitude * Mathf.Cos(t / period);
            float x = radius * Mathf.Sin(theta);
            float y = radius * Mathf.Cos(theta);
            transform.localPosition = new Vector3(x, y, z);
            t += Time.fixedDeltaTime;
        }
    }
}
