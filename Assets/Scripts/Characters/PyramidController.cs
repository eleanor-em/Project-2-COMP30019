using UnityEngine;
using System.Collections.Generic;
using System;

public class PyramidController : MonoBehaviour {
    public float RotSpeed;
    public float HoverSpeed;
    public float HoverAmplitude;
    public float MoveSpeed;
    public float MovePrecision = 0.1f;

    private float t;
    private float yOffset;

    private List<GameObject> markers;
    private GameObject marker = null;

    // Class to sort game objects
    private class MarkerComparer : IComparer<GameObject> {
        private Vector3 position;

        public MarkerComparer(Vector3 position) {
            this.position = position;
        }

        public int Compare(GameObject lhs, GameObject rhs) {
            float distLhs = (lhs.transform.position - position).sqrMagnitude;
            float distRhs = (rhs.transform.position - position).sqrMagnitude;
            return distLhs.CompareTo(distRhs);
        }
    }

    private void NextMarker() {
        if (markers.Count > 0) {
            marker = markers[0];
            markers.RemoveAt(0);
        }
    }

    void Start() {
        // Get list of markers
        markers = new List<GameObject>(FindObjectsOfType<GameObject>());
        markers.RemoveAll(gameObj => gameObj.CompareTag("PyramidMarker") == false);
        // Sort markers by distance
        markers.Sort(new MarkerComparer(transform.position));

        CreateTextbox.Create("Pyramid",
            "Nyeh heh heh! I bet you'll never figure out that you can <color=blue>press Z to jump</color> up these platforms!",
            false, false, answer => NextMarker());
        CreateTextbox.Create("Pyramid",
            "Hmm... I guess that was pretty easy. Now for the piece de resistance -- the moving platform! Bwa ha ha ha ha.",
            false, false, answer => NextMarker());
    }

    void FixedUpdate() {
        yOffset = transform.position.y - HoverAmplitude * Mathf.Sin(HoverSpeed * t);
        t += Time.fixedDeltaTime;
        transform.rotation = Quaternion.Euler(0, RotSpeed * t, 0);
        transform.position = new Vector3(transform.position.x,
                                         yOffset + HoverAmplitude * Mathf.Sin(HoverSpeed * t),
                                         transform.position.z);

        if (marker != null) {
            Vector3 s = (marker.transform.position - transform.position);
            if (s.sqrMagnitude < MovePrecision) {
                Debug.Log("Done");
                marker = null;
            } else {
                transform.position += s.normalized * MoveSpeed * Time.fixedDeltaTime;
            }
        }
    }
}
