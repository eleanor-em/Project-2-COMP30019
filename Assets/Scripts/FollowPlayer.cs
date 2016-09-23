using UnityEngine;

public class FollowPlayer : MonoBehaviour {
    public GameObject Player;
    public float Offset = 4f;
    public float XRot = 20f;

    public float MoveSpeed;
    public float RotSpeed;

    private bool moving = false;
    private Vector3 oldPos;
    private Vector3 targetPos;
    private Quaternion oldRot;
    private Quaternion targetRot;

    private float posLerpTime;
    private float rotLerpTime;

    private PlayerJump jump;

    void Start() {
        jump = Player.GetComponent<PlayerJump>();
    }

	void FixedUpdate () {
        if (moving) {
            transform.position = Vector3.Lerp(oldPos, targetPos, posLerpTime);
            transform.rotation = Quaternion.Lerp(oldRot, targetRot, rotLerpTime);
            posLerpTime += MoveSpeed * Time.fixedDeltaTime;
            rotLerpTime += RotSpeed * Time.fixedDeltaTime;

            if (posLerpTime >= 1 && rotLerpTime >= 1) {
                moving = false;
            }
        }
        // Follow player's y
        transform.position = new Vector3(transform.position.x,
                                         jump.LastGoodY + Offset,
                                         transform.position.z);
    }

    public void RoomChange(Room room) {
        moving = true;
        targetPos = room.CamPosition;
        targetRot = room.CamRotation;

        // Don't move in y
        oldPos = transform.position;
        oldRot = transform.rotation;
        
        // Reset our lerp times here in case a new room is reached while moving
        posLerpTime = 0;
        rotLerpTime = 0;
    }
}
