using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {
    [SerializeField] public int maxHealth = 100;

    public int currentHealth;

    private void Start() {
        currentHealth = maxHealth;
    }

    public void ApplyDamage(int damage) {
        currentHealth -= damage;

        if (currentHealth <= 0) {
            Die();
        }
    }

    private void Die() {
        gameObject.SendMessage("OnDeath");

        // Implement death logic here, such as playing death animations, disabling components, etc.
        Destroy(gameObject);
    }
}
