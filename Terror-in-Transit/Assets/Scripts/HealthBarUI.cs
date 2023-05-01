using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour {
    [SerializeField] private Slider slider;
    [SerializeField] private Health health;

    // Start is called before the first frame update
    private void Start() {
        slider.maxValue = health.maxHealth;
    }

    // Update is called once per frame
    private void Update() {
        slider.value = health.currentHealth;
    }
}
