using UnityEngine;

public class NPCController : MonoBehaviour {
    public string Name;
    public string Text;

    public virtual void Interact(PlayerStamina playerStamina) {
        CreateTextbox.Create(Name, Text);
    }
}
