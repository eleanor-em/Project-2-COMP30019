using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController),
                  typeof(PlayerMove))]
public class PlayerJump : PlayerBehaviour {
    public float JumpSpeed;
    public float Gravity;
    public float WallJumpFactor;

    private float yspeed;
    private CharacterController controller;
    private PlayerMove playerMove;
    private bool touchedWall;
    public bool TouchedWall { get { return touchedWall; } set { touchedWall = value; } }
    private Vector3 wallNormal;

    // Use this for initialization
    void Start () {
        controller = GetComponent<CharacterController>();
        playerMove = GetComponent<PlayerMove>();
    }

    private void Jump() {
        if (!playerMove.Rolling) {
            yspeed = JumpSpeed;
        }
    }

    private void WallJump() {
        if (touchedWall) {
            playerMove.Direction = wallNormal;
            playerMove.AutoMove = true;
            playerMove.AutoMoveSpeed = playerMove.Speed;
            yspeed = JumpSpeed * WallJumpFactor;
        }
    }

    protected override void OnOtherCollision(ControllerColliderHit hit) {
        touchedWall = true;
        wallNormal = hit.normal;
    }

    override protected void OnUpDownCollision(ControllerColliderHit hit) {
        yspeed = 0;
    }

    
    void Update() {
        // Cancel wall jump if the player touched the ground or started to move
        if (playerMove.AutoMove || controller.isGrounded) {
            touchedWall = false;
        }
        if (Input.GetKeyDown(KeyCode.Z)) {
            if (controller.isGrounded) {
                Jump();
            } else {
                WallJump();
            }
        }
    }

    void FixedUpdate () {
        yspeed -= Gravity * Time.fixedDeltaTime;
        // Hack: controller.Move must be handled by playerMove otherwise controller.isGrounded breaks
        playerMove.YSpeed += yspeed;
    }
}
