using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DrawAction : MonoBehaviour {
    public GameObject actionTextObject;
    private Text actionText;

    public GameObject player;
    private PlayerStamina playerStamina;
    private PlayerMove playerMove;

    void Start() {
        playerStamina = player.GetComponent<PlayerStamina>();
        playerMove = player.GetComponent<PlayerMove>();
        actionText = actionTextObject.GetComponent<Text>();
    }

    void Update() {
        actionText.text = "";
        if (CreateTextbox.ShowingBlockingText) {
            actionText.text = "Next";
        }
        else if (!playerMove.Rolling && playerStamina.Stamina >= playerMove.RollStaminaCost) {
            actionText.text = "Roll";
        }
    }
}
