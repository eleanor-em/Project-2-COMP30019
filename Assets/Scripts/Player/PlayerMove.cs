using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class PlayerBehaviour : MonoBehaviour {
    void OnControllerColliderHit(ControllerColliderHit hit) {
        // Separate into two logical cases for platforming
        if (hit.normal == Vector3.up || hit.normal == Vector3.down) {
            OnUpDownCollision(hit);
        } else {
            OnOtherCollision(hit);
        }
    }

    protected virtual void OnUpDownCollision(ControllerColliderHit hit) { }
    protected virtual void OnOtherCollision(ControllerColliderHit hit) { }
}

[RequireComponent(typeof(CharacterController),
                  typeof(PlayerStamina),
                  typeof(PlayerJump))]
public class PlayerMove : PlayerBehaviour {
    private bool speedBoost = false;
    public float BoostRatio = 2f;
    public float BoostTime = 10;
    public float Speed = 4;
    public float YSpeed { get; set; }
    public float rotateSpeed = 0.5f;

    public float RollTime = 0.4f;
    public float RollSpeed = 8;

    public float RollStaminaCost = 40;

    public float TrapShake = 10;
    public float buttonOffset;

    public GameObject moveTargetPrefab;
    public LayerMask CollisionMask;

    public GameObject buttonObject;
    public GameObject leftFoot;
    public GameObject rightFoot;
    
    private Vector3 destination;
    // Unit vector that stores the most recent movement direction
    private Vector3 direction = Vector3.forward;
    public Vector3 Direction { get { return direction; } set { direction = value; } }

    private CharacterController controller;
    private PlayerStamina playerStamina;
    private PlayerJump playerJump;

    private GameObject moveTarget;
    
    // State variables
    private bool rolling = false;
    public bool Rolling { get { return rolling; } }
    private float rollRotateSpeed;
    private float rollAmount;

    private bool moving = false;
    public bool Moving { get { return moving; } }

    public bool AutoMove { get; set; }
    public float AutoMoveSpeed { get; set; }

    // Keep track of chests
    public float chestRange = 1.5f;
    private List<GameObject> chests;
    private bool nearChest = false;
    public bool NearChest { get { return nearChest; } }

    // Keep track of NPCs
    public float NPCRange = 2;
    private List<GameObject> npcs;
    private bool nearNPC = false;
    public bool NearNPC { get { return nearNPC; } }

    public bool Trapped { get; set; }
    private float shakeAmount = 0;

    private IEnumerator Boost() {
        speedBoost = true;
        yield return new WaitForSeconds(BoostTime);
        speedBoost = false;
    }

    // Coroutine to roll for a given period
    private IEnumerator Roll() {
        if (playerStamina.DeductStamina(RollStaminaCost)) {
            AutoMove = true;
            AutoMoveSpeed = RollSpeed;
            rolling = true;
            // Roll for a certain time period
            yield return new WaitForSeconds(RollTime);
            StopRoll();
        }
    }

    private void StopRoll() {
        rolling = false;
        AutoMove = false;
        rollAmount = 0;
        // Update destination (if we weren't still moving)
        if (!moving) {
            destination = transform.position;
        }
    }

    // If we hit a wall
    override protected void OnOtherCollision(ControllerColliderHit hit) {
        // Position comparison is necessary to allow rolling off platforms
        if (rolling && transform.position.y <= hit.transform.position.y) {
            StopRoll();
            // Rolling into a wall means we should stop moving
            destination = transform.position;
        }
    }

    void Start() {
        destination = transform.position;
        controller = GetComponent<CharacterController>();
        playerStamina = GetComponent<PlayerStamina>();
        playerJump = GetComponent<PlayerJump>();

        // Get a list of all the chest objects
        chests = new List<GameObject>(FindObjectsOfType<GameObject>());
        chests.RemoveAll(gameObj => gameObj.CompareTag("Chest") == false);
        // Get a list of all NPC objects
        npcs = new List<GameObject>(FindObjectsOfType<GameObject>());
        npcs.RemoveAll(gameObj => gameObj.CompareTag("NPC") == false);

        // Scale button offset; it was tuned for a height of 353px
        buttonOffset /= 353;
        buttonOffset *= Screen.height;

        // Calculate how many degrees we need to roll per second
        rollRotateSpeed = 360 / RollTime;
    }
    
    private void SetDestination() {
        if (!rolling && (!AutoMove || YSpeed <= 0) && !playerJump.Dying) {
            // Raycast to find where the user clicked
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, float.PositiveInfinity, CollisionMask)) {
                destination = new Vector3(hit.point.x,
                                          transform.position.y,
                                          hit.point.z);
                direction = (destination - transform.position).normalized;

                Destroy(moveTarget);
                moveTarget = Instantiate<GameObject>(moveTargetPrefab);
                moveTarget.transform.position = hit.point;
                // Set up the target for moving platform
                moveTarget.GetComponent<MoveTargetController>().Setup(hit.transform.gameObject);
                // Rotate moveTarget into plane of target
                moveTarget.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }
            moving = true;
            // Take back control of movement
            AutoMove = false;
            playerJump.TouchedWall = false;
        }
    }
    
    private void MoveToDestination() {
        // If we have a target, move to follow it
        if (moveTarget != null) {
            destination = moveTarget.transform.position;
        }
        bool moved = false;
        // Move only in x and z
        destination.Set(destination.x,
                        transform.position.y,
                        destination.z);
        // If we're more than a given distance away, move closer
        if (moving && (destination - transform.position).magnitude > Speed * Time.fixedDeltaTime) {
            // Don't actually move if another routine has control, but don't stop from moving afterwards
            if (!AutoMove) {
                direction = (destination - transform.position).normalized;
                moved = true;
                controller.Move(direction * Speed * Time.fixedDeltaTime
                                * ((speedBoost) ? (BoostRatio) : (1)));
            }
        } else {
            // Check if grounded to handle the case where the player lands on a moving platform
            if (moving && controller.isGrounded) {
                Destroy(moveTarget);
                moveTarget = null;
            }
            destination = transform.position;
            moving = false;
        }
        leftFoot.GetComponent<FootController>().Moving = moved;
        rightFoot.GetComponent<FootController>().Moving = moved;
    }
        
    void Update() {
		if (!(Input.GetKeyDown(KeyCode.X) && CreateTextbox.Continue()) && !Trapped) {
            // Find any closed nearby chests
            nearChest = false;
            GameObject nearbyChest = chests.Find(gameObj =>
                gameObj.GetComponent<ChestController>().Open == false &&
                (transform.position - gameObj.transform.position).magnitude < chestRange
            );
            if (nearbyChest != null) {
                nearChest = true;
            }
            // Find any nearby NPCs
            nearNPC = false;
            GameObject nearbyNPC = npcs.Find(gameObj =>
                (transform.position - gameObj.transform.position).magnitude < NPCRange
            );
            if (nearbyNPC != null) {
                nearNPC = true;
            }

            // Handle X hint
            buttonObject.SetActive(nearChest || nearNPC);
            buttonObject.transform.position = Camera.main.WorldToScreenPoint(transform.position)
                                              + buttonOffset * Vector3.up;

            if (!playerJump.Dying && Input.GetKeyDown(KeyCode.X)) {
                if (!rolling) {
                    if (nearChest) {
                        nearbyChest.GetComponent<ChestController>().OnOpen(playerStamina);
                    } else if (nearNPC) {
                        nearbyNPC.GetComponent<NPCController>().Interact(playerStamina);
                    }
                    else {
                        StartCoroutine("Roll");
                    }
                }
            }

            // On left mouse click
            if (Input.GetMouseButtonDown(0)) {
                SetDestination();
            }

            // Kill non-roll auto move on the ground
            if (AutoMove && !rolling && controller.isGrounded) {
                AutoMove = false;
                destination = transform.position;
            }
        } else {
            shakeAmount += Input.gyro.userAcceleration.magnitude;
            if (shakeAmount > TrapShake) {
                shakeAmount = 0;
                Trapped = false;
            }
        }
    }

    // Do physics here
    void FixedUpdate() {
        // Handle moving platform
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit) && controller.isGrounded) {
            if (hit.collider.CompareTag("MovingPlatform")) {
                controller.Move(hit.collider.GetComponent<MovingPlatform>().Speed * Time.fixedDeltaTime);
            }
        }
        if (!Trapped) {
            if (AutoMove) {
                // Move in a fixed direction if we're rolling
                controller.Move(direction * AutoMoveSpeed * Time.fixedDeltaTime
                                    * ((speedBoost) ? (BoostRatio) : (1)));
            }
            MoveToDestination();
        }
        // Update facing direction
        transform.rotation = Quaternion.AngleAxis(-rollAmount, transform.forward);
        transform.rotation *= Quaternion.Euler(0, -90, 0)
                              * Quaternion.LookRotation(direction);
        // Rotate if we roll
        if (rolling) {
            rollAmount += rollRotateSpeed * Time.fixedDeltaTime;
        }
        // isGrounded fails if Move isn't handled like this. Set to 0 to allow superposition of velocity
        controller.Move(YSpeed * Vector3.up * Time.fixedDeltaTime);
        YSpeed = 0;
        if (!controller.isGrounded) {
            leftFoot.GetComponent<FootController>().Reset();
            rightFoot.GetComponent<FootController>().Reset();
        }
    }
}
