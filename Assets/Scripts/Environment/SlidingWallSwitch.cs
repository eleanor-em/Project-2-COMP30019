using UnityEngine;
using System.Collections;

public class SlidingWallSwitch : SwitchController {
    public GameObject SlidingWall;

    protected override void OnPress() {
        SlidingWall.GetComponent<SlidingWallController>().Move();
    }
    protected override void DePress() {
        SlidingWall.GetComponent<SlidingWallController>().Move();
    }
}
