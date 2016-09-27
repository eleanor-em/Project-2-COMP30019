using UnityEngine;
using System.Collections;

public class PointLight : MonoBehaviour {
    public Color Color;
    public Vector3 Position { get { return transform.position; } set { transform.position = value; } }
}
