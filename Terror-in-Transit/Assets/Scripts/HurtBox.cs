using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtBox : MonoBehaviour {
    [SerializeField] private int damage = 50;

    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent<Health>(out Health health)) {
            health.ApplyDamage(damage);
        }
    }
}
