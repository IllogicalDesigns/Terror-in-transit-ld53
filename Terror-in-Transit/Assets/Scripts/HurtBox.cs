using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtBox : MonoBehaviour {
    [SerializeField] private int damage = 50;
    //[SerializeField] private float cooldownTime = 1f;

    //private float cooldownTimer = 0f;

    //private void Update() {
    //    if (cooldownTimer >= 0) cooldownTimer -= Time.deltaTime;
    //}

    private void OnTriggerEnter(Collider other) {
        //if (cooldownTimer > 0) return;

        if (other.TryGetComponent<Health>(out Health health)) {
            //health.ApplyDamage(damage);
            other.SendMessage("ApplyDamage", damage);
            //cooldownTimer = cooldownTime;
        }
    }
}
