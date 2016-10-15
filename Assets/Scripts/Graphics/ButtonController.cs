using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour {
    public bool Active { get; set; }

    void Update() {
        GetComponent<Image>().enabled = Active;
    }
}
