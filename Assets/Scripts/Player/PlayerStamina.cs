using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public enum Item {
    None,
    PotionOfStamina,
    SpeedBoots,
    Ankh
};

[RequireComponent(typeof(CharacterController),
                  typeof(BlinnPhongShaderControl))]
public class PlayerStamina : MonoBehaviour {
    public float StaminaRegen = 10;
    public float InairRegenFactor = 0.5f;
    public float ImmuneTime = 1;
    public float ImmuneAlpha = 0.5f;
    public int maxHearts = 4;
    public float PotionBonus = 2;
    public float PotionTime = 15;
    public Text GemText;

    private bool potionActive = false;
    public bool PotionActive { get { return potionActive; } }

    public Texture PotionTexture;
    public Texture BootsTexture;
    public Texture AnkhTexture;

    private int hearts;
    public int Hearts { get { return hearts; } }
    private float stamina = 100;
    public float Stamina {  get { return stamina; } }
    private int gems;
    public int Gems { get { return gems; } }

    private CharacterController controller;
    private BlinnPhongShaderControl shader;

    private bool immune;
    public bool Immune { get { return immune; } }

    private Item item = Item.None;
    public Item Item { get { return item; } set { if (item == Item.None) item = value; } }
    public bool HasItem { get { return item != Item.None; } }

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
    }

    private IEnumerator Potion() {
        potionActive = true;
        yield return new WaitForSeconds(PotionTime);
        potionActive = false;
    }

    private void Die() {
        SceneManager.LoadScene("main");
    }

    public void Damage() {
        if (!immune) {
            hearts--;
            StartCoroutine("Immunity");
            if (hearts <= 0) {
                if (item == Item.Ankh) {
                    item = Item.None;
                    hearts = maxHearts;
                } else {
                    Die();
                }
            }
        }
    }

    public void AddGems(int amount) {
        gems += amount;
        if (gems < 0) {
            gems = 0;
        }
    }
    
    void Start() {
        controller = GetComponent<CharacterController>();
        shader = GetComponent<BlinnPhongShaderControl>();
        hearts = maxHearts;
    }

    private void UseItem() {
        switch (item) {
            case Item.PotionOfStamina:
                StartCoroutine("Potion");
                break;
            case Item.SpeedBoots:
                gameObject.GetComponent<PlayerMove>().StartCoroutine("Boost");
                break;
            case Item.Ankh:
                return;
        }
        item = Item.None;
    }

	void Update () {
        // Regenerate stamina
        stamina += ((controller.isGrounded) ? (StaminaRegen) : (StaminaRegen * InairRegenFactor))
                    * Time.deltaTime
                    * ((potionActive) ? (PotionBonus) : (1));
        if (stamina > 100) {
            stamina = 100;
        }

        if (immune) {
            shader.color.a = ImmuneAlpha;
        } else {
            shader.color.a = 1;
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            UseItem();
        }

        GemText.text = gems.ToString();
    }

    void OnGUI() {
        if (item != Item.None) {
            Texture tex = null;
            switch (item) {
                case Item.PotionOfStamina:
                    tex = PotionTexture;
                    break;
                case Item.SpeedBoots:
                    tex = BootsTexture;
                    break;
                case Item.Ankh:
                    tex = AnkhTexture;
                    break;
                default:
                    return;
            }
            Rect r = new Rect(Screen.width / 2 + 3 - tex.width / 2, 3, 100, 100);
            GUI.Label(r, tex);
        }
    }
}
