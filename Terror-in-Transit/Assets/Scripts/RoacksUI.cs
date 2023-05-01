using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RoacksUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private Throwing throwing;

    // Start is called before the first frame update
    private void Start() {
    }

    // Update is called once per frame
    private void Update() {
        textMesh.text = "Rocks: " + throwing.ammo;
    }
}
