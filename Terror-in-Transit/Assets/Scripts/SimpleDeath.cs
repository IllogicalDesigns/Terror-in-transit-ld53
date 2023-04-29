using UnityEngine;
using UnityEngine.Events;

public class SimpleDeath : MonoBehaviour {
    [SerializeField] private UnityEvent onDeath;

    public void OnDeath() {
        onDeath.Invoke();
        Destroy(gameObject);
    }
}
