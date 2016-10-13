using UnityEngine;
using System.Collections;

public class ChestController : MonoBehaviour {
    public int gems;
    public Mesh openMesh;

    private bool open = false;
    public bool Open { get { return open; } }

    public void OnOpen(PlayerStamina player) {
        if (!open) {
            player.AddGems(gems);
            open = true;
            GetComponent<MeshFilter>().mesh = openMesh;
            CreateTextbox.Create("Chest", "You found <color=green>" + gems.ToString() + " gems</color>!");
        }
    }
}
