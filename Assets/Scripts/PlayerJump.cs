using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController),
                  typeof(PlayerMove),
                  typeof(PlayerStamina))]
public class PlayerJump : PlayerBehaviour {
    public float JumpSpeed;
    public float Gravity;
    public float WallJumpCost;
    public float WallJumpFactor;
    public float DeathTime = 2;

    private float yspeed;

    private CharacterController controller;
    private PlayerMove playerMove;
    private PlayerStamina playerStamina;

    private bool touchedWall;
    public bool TouchedWall { get { return touchedWall; } set { touchedWall = value; } }
    private Vector3 wallNormal;

    private GameObject lastTouched;
    private bool dying = false;
    public bool Dying { get { return dying; } }
    private float lastGoodY;
    public float LastGoodY { get { return lastGoodY; } }

    private bool jump = false;

    // Use this for initialization
    void Start () {
        controller = GetComponent<CharacterController>();
        playerMove = GetComponent<PlayerMove>();
        playerStamina = GetComponent<PlayerStamina>();
    }

    private void Jump() {
        if (!playerMove.Rolling) {
            yspeed = JumpSpeed;
        }
    }

    private void WallJump() {
        if (touchedWall && playerStamina.DeductStamina(WallJumpCost)) {
            touchedWall = false;
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

    // Needed to deal with the case where the player jumps while underneath a platform
    override protected void OnUpDownCollision(ControllerColliderHit hit) {
        yspeed = 0;
        // Store last solid ground
        if (hit.normal == Vector3.up && !dying) {
            lastTouched = hit.gameObject;
        }
    }

    private IEnumerator Die() {
        dying = true;
        yield return new WaitForSeconds(DeathTime);
        dying = false;
        transform.position = lastTouched.transform.position + Vector3.up * 0.5f;
        yield break;
    }

    void OnTriggerEnter(Collider collider) {
        if (collider.gameObject.CompareTag("Hazard")) {
            playerStamina.Damage();
            StartCoroutine("Die");
        }
    }
        
    void Update() {
        // Cancel wall jump if the player touched the ground or started to move
        if (playerMove.Rolling || controller.isGrounded) {
            touchedWall = false;
        }
        if (Input.GetKeyDown(KeyCode.Z)) {
            if (controller.isGrounded) {
                jump = true;
            } else {
                WallJump();
            }
        }
    }

    void FixedUpdate () {
        if (controller.isGrounded) {
            yspeed = 0;
            if (jump) {
                Jump();
                jump = false;
            }
        }
        yspeed -= Gravity * Time.fixedDeltaTime;
        // Hack: controller.Move must be handled by playerMove otherwise controller.isGrounded breaks
        playerMove.YSpeed += yspeed;
    }

    void LateUpdate() {
        // Store the last y from before we died so that the camera follows us smoothly
        if (!dying) {
            lastGoodY = transform.position.y;
        }
    }
}
