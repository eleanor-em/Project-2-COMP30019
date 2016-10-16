using UnityEngine;
using System.Collections;

public class ChestController : MonoBehaviour {
    public int gems;
    public Mesh openMesh;
    public Texture openTex;
    public Texture openMap;

    private bool open = false;
    public bool Open { get { return open; } }
    protected virtual void Opened() { }
    public void OnOpen(PlayerStamina player) {
        if (!open) {
            player.AddGems(gems);
            open = true;
            GetComponent<MeshFilter>().mesh = openMesh;
            Material material = GetComponent<MeshRenderer>().material;
            material.SetTexture("_MainTex", openTex);
            material.SetTexture("_BumpMap", openMap);
            
            CreateTextbox.Create("Chest", "You found <color=blue>" + gems.ToString() + " gems</color>!");
            Opened();
        }
    }
}
