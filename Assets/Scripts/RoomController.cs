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

    public override int GetHashCode() {
        return name.GetHashCode();
    }

    public override bool Equals(object obj) {
        if (obj is Room) {
            return name.Equals((obj as Room).name);
        }
        return false;
    }

    public static bool operator == (Room lhs, Room rhs) {
        return lhs.Equals(rhs);
    }
    public static bool operator != (Room lhs, Room rhs) {
        return !(lhs == rhs);
    }
}

public class RoomController : MonoBehaviour {
    private Room room;
    public Room Room { get { return room; } }
    public LayerMask collisionMask;
    
	void Update() {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 100, collisionMask.value)) {
            Room next = hit.transform.gameObject.GetComponent<RoomContainer>().Room;
            if (next != room) {
                room = next;
                Camera.main.GetComponent<FollowPlayer>().RoomChange(room);
            }
        }
	}
}
