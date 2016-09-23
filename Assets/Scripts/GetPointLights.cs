using UnityEngine;
using System.Collections;

public class GetPointLights : MonoBehaviour {
    public PointLight[] Get() {
        return GetComponentsInChildren<PointLight>();
    }
}
