using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnExitTrigger : MonoBehaviour {
    [SerializeField] private UnityEvent onTriggerEnter;
    [SerializeField] private string requiredTag = "";

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag(requiredTag))
            onTriggerEnter.Invoke();
    }
}
