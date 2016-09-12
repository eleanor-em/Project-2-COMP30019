using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerMove : MonoBehaviour {
    public float Speed;
    public float JumpSpeed;
    public float Gravity;

    public float RollTime;
    public float RollSpeed;

    public float StaminaRegen;
    public float InairRegenFactor;
    public float RollStaminaCost;

    public Text staminaText;
    
    private Vector3 destination;
    // Unit vector that stores the most recent movement direction
    private Vector3 direction = Vector3.forward;

    private CharacterController controller;
    private float yspeed;

    // Only check for Collision objects
    private int CollisionMask;

    // State variables
    private bool rolling = false;
    private bool moving = false;
    private float stamina = 100;

    // Coroutine to roll for a given period
    private IEnumerator Roll() {
        if (!rolling && stamina > RollStaminaCost) {
            rolling = true;
            stamina -= RollStaminaCost;
            // Roll for a certain time period
            yield return new WaitForSeconds(RollTime);
            StopRoll();
        }
        yield break;
    }

    private void StopRoll() {
        rolling = false;
        // Update destination (if we weren't already moving)
        if (!moving) {
            destination = transform.position;
        }
    }

    private void Jump() {
        if (!rolling && controller.isGrounded) {
            yspeed = JumpSpeed;
        }
    }

    private void OnUpDownCollision(ControllerColliderHit hit) {
        yspeed = 0;
    }

    // If we hit a wall
    private void OnOtherCollision(ControllerColliderHit hit) {
        if (rolling) {
            StopRoll();
            // Rolling into a wall means we should stop moving
            destination = transform.position;
            if (moving) {
                ///TODO: Generate impact with wall here
            }
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit) {
        // Separate into two logical cases for platforming
        if (hit.normal == Vector3.up || hit.normal == Vector3.down) {
            OnUpDownCollision(hit);
        } else {
            OnOtherCollision(hit);
        }
    }

    void Start() {
        destination = transform.position;
        controller = GetComponent<CharacterController>();
        CollisionMask = 1 << LayerMask.NameToLayer("Collision");
    }
    
    private void SetDestination() {
        if (!rolling) {
            // Raycast to find where the user clicked
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, CollisionMask)) {
                destination = new Vector3(hit.point.x,
                                          transform.position.y,
                                          hit.point.z);
                direction = (destination - transform.position).normalized;
            }
        }
    }
    
    private void MoveToDestination() {
        // Move only in x and z
        destination.Set(destination.x,
                        transform.position.y,
                        destination.z);
        // If we're more than a given distance away, move closer
        if ((destination - transform.position).magnitude > Speed * Time.fixedDeltaTime) {
            controller.Move(direction * Speed * Time.fixedDeltaTime);
            moving = true;
        } else {
            transform.position = destination;
            moving = false;
        }
    }
        
    void Update() {
        if (Input.GetKeyDown(KeyCode.Z)) { 
            Jump();
        }
        if (Input.GetKeyDown(KeyCode.X)) {
            StartCoroutine("Roll");
        }

        // On left mouse click
        if (Input.GetMouseButtonDown(0)) {
            SetDestination();
        }

        // Regenerate stamina
        stamina += ((controller.isGrounded) ? (StaminaRegen) : (StaminaRegen * InairRegenFactor))
                    * Time.deltaTime;
        if (stamina > 100) {
            stamina = 100;
        }

        staminaText.text = "Stamina: " + (int)stamina + "%";
    }

    // Do physics here
    void FixedUpdate() {
        if (rolling) {
            // Move in a fixed direction if we're rolling
            controller.Move(direction * RollSpeed * Time.fixedDeltaTime);
        } else {
            MoveToDestination();
        }
        yspeed -= Gravity * Time.fixedDeltaTime;
        controller.Move(Vector3.up * yspeed * Time.fixedDeltaTime);
    }
}
