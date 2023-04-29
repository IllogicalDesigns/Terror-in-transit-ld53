using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractionText : MonoBehaviour {
    public static InteractionText instance;
    private TextMeshProUGUI textMesh;

    // Start is called before the first frame update
    private void Start() {
        if (instance != null) Debug.LogError("Too many interaction texts");

        instance = this;

        textMesh = gameObject.GetComponent<TextMeshProUGUI>();
    }

    public void SetText(string newString) {
        textMesh.text = newString;
    }
}
