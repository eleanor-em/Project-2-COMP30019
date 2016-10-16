using UnityEngine;
using System.Collections;

public class ChestWallController : ChestController {
    public GameObject wall;

    protected override void Opened() {
        wall.GetComponent<SlidingWallController>().Move();
    }
}
