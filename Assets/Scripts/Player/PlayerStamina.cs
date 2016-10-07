using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CharacterController),
                  typeof(BlinnPhongShaderControl))]
public class PlayerStamina : MonoBehaviour {
    public float StaminaRegen = 10;
    public float InairRegenFactor = 0.5f;
    public float ImmuneTime = 1;
    public float ImmuneAlpha = 0.5f;
    public int maxHearts = 4;

    private int hearts;
    public int Hearts { get { return hearts; } }
    private float stamina = 100;
    public float Stamina {  get { return stamina; } }
    private CharacterController controller;
    private BlinnPhongShaderControl shader;

    private bool immune;
    public bool Immune { get { return immune; } }
	
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

    private IEnumerator Immunity() {
        immune = true;
        yield return new WaitForSeconds(ImmuneTime);
        immune = false;
        yield break;
    }

    private void Die() {
        //// TODO: Have player die if they run out of hearts
        hearts = maxHearts;
    }

    public void Damage() {
        if (!immune) {
            hearts--;
            StartCoroutine("Immunity");
            if (hearts <= 0) {
                Die();
            }
        }
    }
    
    void Start() {
        controller = GetComponent<CharacterController>();
        shader = GetComponent<BlinnPhongShaderControl>();
        hearts = maxHearts;
    }

	void Update () {
        // Regenerate stamina
        stamina += ((controller.isGrounded) ? (StaminaRegen) : (StaminaRegen * InairRegenFactor))
                    * Time.deltaTime;
        if (stamina > 100) {
            stamina = 100;
        }

        if (immune) {
            shader.color.a = ImmuneAlpha;
        } else {
            shader.color.a = 1;
        }
    }
}
