using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnEnterTrigger : MonoBehaviour {
    [SerializeField] private UnityEvent onTriggerEnter;
    [SerializeField] private string requiredTag = "";

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag(requiredTag))
            onTriggerEnter.Invoke();
    }
}
