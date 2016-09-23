using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerStamina : MonoBehaviour {
    public float StaminaRegen = 10;
    public float InairRegenFactor = 0.5f;
    public Text staminaText;

    private float stamina = 100;
    private CharacterController controller;
	
    /// <summary>
    /// Deduct the given amount of stamina.
    /// </summary>
    /// <returns>
    /// Whether the stamina was high enough to deduct the amount
    /// </returns>
    public bool DeductStamina(float amount) {
        if (stamina >= amount) {
            stamina -= amount;
            return true;
        }
        return false;
    }
    
    void Start() {
        controller = GetComponent<CharacterController>();
    }

	void Update () {
        // Regenerate stamina
        stamina += ((controller.isGrounded) ? (StaminaRegen) : (StaminaRegen * InairRegenFactor))
                    * Time.deltaTime;
        if (stamina > 100) {
            stamina = 100;
        }
        staminaText.text = "Stamina: " + (int)stamina + "%";
    }
}
