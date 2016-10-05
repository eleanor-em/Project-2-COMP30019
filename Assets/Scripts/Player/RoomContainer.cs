using UnityEngine;
using System.Collections;

public class RoomContainer : MonoBehaviour {
    public Room Room;
    
    public Vector3 CamPos;
    public float CamYRot;

    void Start() {
        Room = new Room(gameObject.name, CamPos, Quaternion.AngleAxis(CamYRot, Vector3.up)
                        * Quaternion.AngleAxis(Camera.main.GetComponent<FollowPlayer>().XRot,
                        Vector3.right));
    }
}
