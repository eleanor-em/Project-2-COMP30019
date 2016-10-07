using UnityEngine;
using System.Collections;

public class CreateTextbox {
    public static void Create() {
        GameObject obj = new GameObject();
        Textbox textbox = obj.AddComponent<Textbox>();
        textbox.setup();
    }
}

public class Textbox : MonoBehaviour {
    private Texture2D textboxLeft;
    private Texture2D textboxMiddle;
    private Texture2D textboxRight;
    private float leftPadding = 10;
    private float bottomPadding = 20;

    public void setup() {
        textboxLeft = Resources.Load<Texture2D>("Textures/textboxLeft");
        textboxMiddle = Resources.Load<Texture2D>("Textures/textbox");
        textboxRight = Resources.Load<Texture2D>("Textures/textboxRight");
    }

    void OnGUI() {
        // Draw left
        Rect r = new Rect(leftPadding, Screen.height - textboxLeft.height - bottomPadding,
                          textboxLeft.width, textboxLeft.height);
        GUI.DrawTexture(r, textboxLeft);
        // Draw middle
        r = new Rect(leftPadding + textboxLeft.width, Screen.height - textboxMiddle.height - bottomPadding,
                     Screen.width - 2 * leftPadding - textboxLeft.width - textboxRight.width,
                     textboxMiddle.height);
        GUI.DrawTexture(r, textboxMiddle);
        // Draw right
        r = new Rect(Screen.width - leftPadding - textboxRight.width,
                     Screen.height - textboxRight.height - bottomPadding,
                     textboxRight.width, textboxRight.height);
        GUI.DrawTexture(r, textboxRight);
    }
}
