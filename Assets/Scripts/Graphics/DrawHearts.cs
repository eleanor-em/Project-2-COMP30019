using UnityEngine;
using System.Collections;

public class DrawHearts : MonoBehaviour {
    public GameObject player;
    public Texture FullHeartTexture;
    public Texture EmptyHeartTexture;
    public Texture emptyStaminaTexture;
    public Texture staminaTexture;
    public float xOffset;
    public float yOffset;

    private PlayerStamina stamina;
    
	void Start () {
        stamina = player.GetComponent<PlayerStamina>();
	}
	
    void OnGUI() {
        // Draw hearts
        Rect r = new Rect(xOffset, yOffset, Screen.width, Screen.height);
        GUILayout.BeginArea(r);
        GUILayout.BeginHorizontal();
        for (int i = 0; i < stamina.maxHearts; ++i) {
            GUILayout.Label((i < stamina.Hearts) ? (FullHeartTexture) : (EmptyHeartTexture));
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
        // Draw stamina bar
        r = new Rect(5, 42, 160, 12);
        GUI.DrawTexture(r, emptyStaminaTexture);
        // Draw percentage
        r = new Rect(6, 43, 158 * stamina.Stamina / 100, 10);
        GUI.DrawTexture(r, staminaTexture);
    }
}
