using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnStayTrigger : MonoBehaviour {
    [SerializeField] private UnityEvent onTriggerStay;
    [SerializeField] private string requiredTag = "";

    private void OnTriggerStay(Collider other) {
        if (other.CompareTag(requiredTag))
            onTriggerStay.Invoke();
    }
}
