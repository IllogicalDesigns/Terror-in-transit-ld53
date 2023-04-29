using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionRay : MonoBehaviour {
    public float distance = 10f;
    public Vector3 size = new Vector3(1f, 1f, 1f);
    public LayerMask layerMask;

    private Transform cam;

    private Interactable currentInteraction;

    private void Start() {
        cam = Camera.main.transform;
    }

    private void Update() {
        // Get the position and forward direction of the camera
        Vector3 origin = cam.position;
        Vector3 direction = cam.forward;

        // Perform the boxcast
        RaycastHit hit;
        if (Physics.BoxCast(origin, size / 2f, direction, out hit, cam.rotation, distance, layerMask)) {
            if (hit.collider.TryGetComponent<Interactable>(out Interactable interactable)) {
                interactable.OnViewed("Press 'F' to ");
                currentInteraction = interactable;
            }
        }
        else {
            if (currentInteraction != null) currentInteraction.ClearText();
            currentInteraction = null;
        }

        if (currentInteraction != null && Input.GetKeyDown(KeyCode.F)) {
            currentInteraction.OnInteraction();
        }
    }
}
