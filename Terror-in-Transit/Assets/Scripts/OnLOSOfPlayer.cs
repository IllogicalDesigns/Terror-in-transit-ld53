using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnLOSOfPlayer : MonoBehaviour {
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask layerMask;

    [SerializeField] private UnityEvent onPlayerSeesUs;
    [SerializeField] private float range = 30f;

    // Start is called before the first frame update
    private void Start() {
        player = FindObjectOfType<PlayerMovement>().transform;
    }

    // Update is called once per frame
    private void Update() {
        if (inRange() && isInFront(transform.position) && hasLOS()) {
            onPlayerSeesUs.Invoke();
        }

        bool inRange() {
            return Vector3.Distance(transform.position, player.position) < range;
        }

        bool isInFront(Vector3 targetVector) {
            Vector3 playerPosition = player.position;
            Vector3 directionToVector = targetVector - playerPosition;
            float dotProduct = Vector3.Dot(directionToVector, transform.forward);

            if (dotProduct < 0)
                return true;
            else
                return false;
        }

        bool hasLOS() {
            return !Physics.Linecast(transform.position, player.position, layerMask);
        }
    }
}
