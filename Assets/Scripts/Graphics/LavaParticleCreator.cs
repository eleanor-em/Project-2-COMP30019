using UnityEngine;
using System.Collections;

public class LavaParticleCreator : MonoBehaviour {
    public GameObject Particles;
    public GameObject Light;
    public float step = 2f;

    void Start() {
        Bounds bounds = GetComponent<MeshRenderer>().bounds;
        Instantiate(Light, bounds.center + new Vector3(0, 2f, 0), Quaternion.identity);
        // Subtract 1 to avoid overlapping other objects
        int xSize = (int)(bounds.size.x / 2) - 1;
        int zSize = (int)(bounds.size.z / 2) - 1;
        for (float i = -xSize; i <= xSize; i += step) {
            for (float j = -zSize; j <= zSize; j += step) {
                Instantiate(Particles, bounds.center + new Vector3(i, -0.25f, j),
                            Quaternion.AngleAxis(90, Vector3.left));
            }
        }
    }
}
