using UnityEngine;
using System.Collections;

public class Room {
    private string name;
    private Vector3 camPosition;
    private Quaternion camRotation;

    public string Name { get { return name; } }
    public Vector3 CamPosition { get { return camPosition; } }
    public Quaternion CamRotation { get { return camRotation; } }

    public Room(string name, Vector3 camPos, Quaternion camRot) {
        this.name = name;
        camPosition = camPos;
        camRotation = camRot;
    }
}

public class RoomController : MonoBehaviour {
    private Room room;
    public Room Room { get { return room; } }

    private int CollisionMask;

    void Start() {
        CollisionMask = 1 << LayerMask.NameToLayer("RoomControl");
    }
    
	void Update() {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, CollisionMask)) {
            room = hit.transform.gameObject.GetComponent<RoomContainer>().Room;
        }
	}
}
