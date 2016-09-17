using UnityEngine;
using System.Collections;
using System;

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

	void FixedUpdate () {
        if (moving) {
            transform.position = Vector3.Lerp(oldPos, targetPos, posLerpTime);
            transform.rotation = Quaternion.Lerp(oldRot, targetRot, rotLerpTime);
            posLerpTime += MoveSpeed * Time.fixedDeltaTime;
            rotLerpTime += RotSpeed * Time.fixedDeltaTime;

            if (posLerpTime >= 1 && rotLerpTime >= 1) {
                moving = false;
                posLerpTime = 0;
                rotLerpTime = 0;
            }
        }
        // Follow player's y
        transform.position = new Vector3(transform.position.x,
                                         Player.transform.position.y + Offset,
                                         transform.position.z);
    }

    public void RoomChange(Room room) {
        moving = true;
        targetPos = room.CamPosition;
        targetRot = room.CamRotation;
        // Don't move in y
        oldPos = transform.position;
        oldPos.Scale(new Vector3(1, 0, 1));
        oldRot = transform.rotation;
    }
}
