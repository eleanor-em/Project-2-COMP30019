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
    public float Speed = 4;
    public float YSpeed { get; set; }

    public float RollTime = 0.4f;
    public float RollSpeed = 8;

    public float RollStaminaCost = 40;

    public float TrapShake = 10;

    public GameObject moveTargetPrefab;
    public LayerMask CollisionMask;
    
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

    private bool moving = false;
    public bool Moving { get { return moving; } }

    public bool AutoMove { get; set; }
    public float AutoMoveSpeed { get; set; }

    // Keep track of chests
    public float chestRange = 10;
    private List<GameObject> chests;
    private bool nearChest = false;
    public bool NearChest { get { return nearChest; } }

    public bool Trapped { get; set; }
    private float shakeAmount = 0;

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
        /*CreateTextbox.Create("You", new string[]
                             { "Lorem ipsum dolor sit amet, labore mandamus at per, debet invidunt in mei. Mel mucius argumentum no, ponderum suavitate ex eam. Ex lorem malis legimus his, his aperiam feugait nostrum ex. Ea qui veritus insolens perpetua. Ut eam prima persecuti, nihil nullam at mei. Te eos mucius pertinacia definitionem, ut tation invidunt per. Ex ferri rebum quo, sumo aliquip ius no. Definiebas adversarium te vim, mutat viris vidisse ut est. Ex nullam tamquam fastidii quo. Habeo feugait no eam. Per in affert similique, nec hinc paulo quidam ea. Ne has delicata splendide sententiae, ea vel quod mazim. Has no facer eruditi honestatis. At pro eros adversarium. Te mel voluptua consectetuer. Eros harum accusam no pro. Ut pro prima delicatissimi, vis detracto evertitur ea, his unum forensibus in. Tollit sapientem temporibus et sit, pri latine oportere eu, veritus apeirian sea et. Singulis sensibus forensibus et pro." }
                            );*/
        // Get a list of all the chest objects
        chests = new List<GameObject>(FindObjectsOfType<GameObject>());
        chests.RemoveAll(gameObj => gameObj.CompareTag("Chest") == false);
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
        // Move only in x and z
        destination.Set(destination.x,
                        transform.position.y,
                        destination.z);
        // If we're more than a given distance away, move closer
        if (moving && (destination - transform.position).magnitude > Speed * Time.fixedDeltaTime) {
            // Don't actually move if another routine has control, but don't stop from moving afterwards
            if (!AutoMove) {
                direction = (destination - transform.position).normalized;
                controller.Move(direction * Speed * Time.fixedDeltaTime);
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
    }
        
    void Update() {
        if (!Trapped) {
            // Find any closed nearby chests
            nearChest = false;
            List<GameObject> nearbyChests = chests.FindAll(gameObj =>
                gameObj.GetComponent<ChestController>().Open == false &&
                (transform.position - gameObj.transform.position).magnitude < chestRange
            );
            if (nearbyChests.Count > 0) {
                nearChest = true;
            }

            if (Input.GetKeyDown(KeyCode.X)) {
                if (!rolling && !CreateTextbox.Continue()) {
                    if (nearChest) {
                        nearbyChests[0].GetComponent<ChestController>().OnOpen(playerStamina);
                    } else {
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
            shakeAmount += Input.acceleration.magnitude;
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
                controller.Move(hit.collider.GetComponent<MovingPlatform>().Speed);
            }
        }
        if (AutoMove) {
            // Move in a fixed direction if we're rolling
            controller.Move(direction * AutoMoveSpeed * Time.fixedDeltaTime);
        }
        MoveToDestination();
        // isGrounded fails if Move isn't handled like this. Set to 0 to allow superposition of velocity
        controller.Move(YSpeed * Vector3.up * Time.fixedDeltaTime);
        YSpeed = 0;
    }
}
