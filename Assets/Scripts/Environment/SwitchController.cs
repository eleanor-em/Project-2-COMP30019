using UnityEngine;
using System.Collections;

public class SwitchController : MonoBehaviour {
    public float Duration = 0;
    public float ScaleSpeed = 0.1f;
    public float MinScale = 0.1f;
    private float yScale;
    private int scaling = 0;

    void Start() {
        yScale = transform.localScale.y;
    }

    void FixedUpdate() {
        float target = (scaling > 0) ? (MinScale) : (yScale);
        if (scaling != 0) {
            // If we're not "close enough" to done..
            if (Mathf.Abs(transform.localScale.y - target) > 0.001f) {
                transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(transform.localScale.x,
                                                                                      target,
                                                                                      transform.localScale.z),
                                                    ScaleSpeed);
            } else if (scaling < 0) {
                scaling = 0;
            }
        }
    }

    private IEnumerator PressTimer() {
        if (Duration != 0) {
            yield return new WaitForSeconds(Duration);
            DePress();
            scaling = -1;
        }
    }
    protected virtual void DePress() {

    }
    protected virtual void OnPress() {

    }
    public void Press() {
        if (scaling == 0) {
            StartCoroutine("PressTimer");
            scaling = 1;
            OnPress();
        }
    }
}
