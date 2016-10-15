using UnityEngine;
using System.Collections;

public class ItemController : NPCController {
    public float RotSpeed = 200;
    public int price;
    public Item item;

    public override void Interact(PlayerStamina playerStamina) {
        CreateTextbox.Create(Name, Text + "\nThis costs <color=blue>" + price + " gems</color>.");
        CreateTextbox.Create(Name, new string[] { "Buy", "Cancel" }, true, true,
            selection => {
                switch (selection) {
                    // Buy
                    case 0:
                        if (playerStamina.Gems >= price) {
                            if (playerStamina.HasItem) {
                                CreateTextbox.Create(Name, "You're already holding an item...");
                            } else {
                                playerStamina.AddGems(-price);
                                playerStamina.Item = item;
                                CreateTextbox.Create(Name, "Come again!");
                            }
                        } else {
                            CreateTextbox.Create(Name, "You don't have enough <color=blue>gems</color>...");
                        }
                        break;
                    // Cancel
                    case 1:
                        CreateTextbox.Create(Name, "Come again!");
                        break;
                }
            });
    }
    
    void Update() {
        transform.Rotate(Vector3.up, RotSpeed * Time.deltaTime);
    }
}
