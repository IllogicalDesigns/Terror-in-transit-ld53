using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour {
    [SerializeField] private string interactText = "";
    [SerializeField] private UnityEvent onInteraction;

    public void OnInteraction() {
        onInteraction.Invoke();
    }

    public void OnViewed(string preText = "") {
        var str = preText + interactText;
        InteractionText.instance.SetText(str);
    }

    public void ClearText() {
        InteractionText.instance.SetText("");
    }
}
