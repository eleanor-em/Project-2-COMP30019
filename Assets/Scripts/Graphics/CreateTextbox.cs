using UnityEngine;
using System.Collections.Generic;

public class CreateTextbox {
    private struct TextStruct {
        public string[] text;
        public string name;
        public bool blocking;
    }
    private static Queue<TextStruct> textboxes;
    private static GameObject gameObject;

    private static bool showingText = false;

    public static bool ShowingBlockingText { get { return showingText && gameObject.GetComponent<Textbox>().Blocking; } }

    public static void Create(string name, string[] text, bool blocking = true) {
        // Set up static instances if we haven't yet
        if (gameObject == null) {
            gameObject = new GameObject();
            textboxes = new Queue<TextStruct>();
        }
        // If we have no current textboxes, create one
        if (!showingText) {
            showingText = true;
            gameObject.AddComponent<Textbox>()
                      .Setup(name, text, blocking);
        } else {
            // If we do have a textbox, add it to the queue
            TextStruct textStruct = new TextStruct();
            textStruct.name = name;
            textStruct.text = text;
            textStruct.blocking = blocking;
            textboxes.Enqueue(textStruct);
        }
    }

    private static void NextTextbox() {
        if (textboxes.Count != 0) {
            TextStruct text = textboxes.Dequeue();
            gameObject.AddComponent<Textbox>()
                      .Setup(text.name, text.text, text.blocking);
        } else {
            showingText = false;
        }
    }

    // Returns true if text existed
    public static bool Continue() {
        Textbox textbox = gameObject.GetComponent<Textbox>();
        if (textbox != null) {
            if (textbox.Blocking) {
                // If this textbox is done, start the next one
                if (textbox.Continue()) {
                    NextTextbox();
                }
                return true;
            }
        }
        return false;
    }

    public static void Close() {
        Textbox textbox = gameObject.GetComponent<Textbox>();
        textbox.Close();
        NextTextbox();
    }
}

public class Textbox : MonoBehaviour {
    private Texture2D textboxLeft;
    private Texture2D textboxMiddle;
    private Texture2D textboxRight;
    private Texture2D nextPromptDone;
    private Texture2D nextPromptNext;

    private readonly float leftPadding = Screen.height / 5.75f;
    private const float bottomPadding = 20;
    private const float textLeftPadding = 15;
    private const float textTopPadding = 10;
    private const float scrollRate = 100f;

    private GUIStyle textStyle;
    private float textWidth;
    private float textHeight;

    private List<string> text;

    private int textIndex = 0;
    private float currentChar = 0;

    private const float blinkRate = 0.6f;
    private float time = 0;
    private bool drawNext = true;

    private bool blocking;
    public bool Blocking { get { return blocking; } }

    public void Close() {
        Destroy(this);
    }

    // Moves the cursor to the end of the name
    private void resetCurrentChar() {
        currentChar = 0;
        while (text[textIndex][(int)currentChar] != '\n') {
            ++currentChar;
        }
    }

    // Returns true if the text is finished
    public bool Continue() {
        if (blocking) {
            // Skip to the end if we haven't finished, go to next line if we have
            if (currentChar >= text[textIndex].Length) {
                textIndex++;
                if (textIndex >= text.Count) {
                    Destroy(this);
                    return true;
                }
                resetCurrentChar();
            } else {
                currentChar = text[textIndex].Length;
            }
        }
        return false;
    }

    public void Setup(string name = "Unknown", string[] text = null, bool blocking = true) {
        if (text == null) {
            text = new string[] { };
        }
        // Load textures
        textboxLeft = Resources.Load<Texture2D>("Textures/textboxLeft");
        textboxMiddle = Resources.Load<Texture2D>("Textures/textbox");
        textboxRight = Resources.Load<Texture2D>("Textures/textboxRight");
        nextPromptDone = Resources.Load<Texture2D>("Textures/endText");
        nextPromptNext = Resources.Load<Texture2D>("Textures/nextText");
        // Set up variables
        this.name = name;
        this.text = new List<string>(text);
        this.blocking = blocking;
        // Set up styles, and cut text if we need to
        textStyle = new GUIStyle();
        textStyle.richText = true;
        textStyle.fontSize = 22;
        textStyle.wordWrap = true;
        // Calculate where to cut the string
        textWidth = Screen.width - 2 * leftPadding - textboxLeft.width - textboxRight.width
                          - 2 * textLeftPadding;
        textHeight = textboxMiddle.height - 2 * textTopPadding;

        for (int i = 0; i < this.text.Count; ++i) {
            this.text[i] = "<b>" + name + "</b>:\n" + this.text[i];
            Vector2 size = textStyle.CalcSize(new GUIContent(this.text[i]));

            // Calculate how many lines this results in
            int lines = (int)(size.x / textWidth);
            // Split the textbox if we need to
            if (lines * size.y > textHeight) {
                // Calculate the width we would need, and use it as an index by dividing by approximate pixel width
                int cutIndex = (int)(textHeight / size.y * textWidth / 7.2f);
                while (this.text[i][cutIndex] != ' ') {
                    --cutIndex;
                }
                // Split the string here
                string oldStr = this.text[i].Substring(0, cutIndex);
                // Add 1 to skip the space
                string newStr = this.text[i].Substring(cutIndex + 1, this.text[i].Length - cutIndex - 1);
                this.text[i] = oldStr;
                this.text.Insert(i + 1, newStr);
            }
        }
        resetCurrentChar();
    }

    void Update() {
        currentChar += scrollRate * Time.deltaTime;
        if (currentChar > text[textIndex].Length) {
            currentChar = text[textIndex].Length;
        }
        time += Time.deltaTime;
        if (time > blinkRate) {
            time = 0;
            drawNext = !drawNext;
        }
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
        
        // Calculate the string
        string toPrint = text[textIndex].Substring(0, (int)currentChar);

        // Draw the string
        r = new Rect(leftPadding + textLeftPadding,
                     Screen.height - bottomPadding - textboxMiddle.height + textTopPadding,
                     textWidth, textHeight);
        GUI.Label(r, toPrint, textStyle);

        // Draw next prompt
        r = new Rect(textWidth + leftPadding + textLeftPadding,
                     Screen.height - bottomPadding - textboxMiddle.height + textTopPadding + textHeight
                        - nextPromptNext.height,
                     nextPromptNext.width, nextPromptNext.height);
        if ((int)currentChar == text[textIndex].Length) {
            // Draw rectangle if we're on the last box
            if (textIndex == text.Count - 1) {
                GUI.DrawTexture(r, nextPromptDone);
            } else if (drawNext) {
                GUI.DrawTexture(r, nextPromptNext);
            }
        }
    }
}
