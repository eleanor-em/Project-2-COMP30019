using UnityEngine;
using System.Collections;

public class DrawHearts : MonoBehaviour {
    public GameObject player;
    public Texture FullHeartTexture;
    public Texture EmptyHeartTexture;
    public float xOffset;
    public float yOffset;

    private PlayerStamina stamina;
    
	void Start () {
        stamina = player.GetComponent<PlayerStamina>();
	}
	
    void OnGUI() {
        Rect r = new Rect(xOffset, yOffset, Screen.width, Screen.height);
        GUILayout.BeginArea(r);
        GUILayout.BeginHorizontal();
        for (int i = 0; i < stamina.maxHearts; ++i) {
            GUILayout.Label((i < stamina.Hearts) ? (FullHeartTexture) : (EmptyHeartTexture));
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}
