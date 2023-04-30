using UnityEngine;

public class Crouch : MonoBehaviour {
    public CharacterController controller;
    public float crouchHeight = 1.0f;
    public float normalHeight = 2.0f;

    private void OnEnable() {
    }

    private void OnDisable() {
    }

    private void Update() {
        if (Input.GetKey(KeyCode.LeftControl)) {
            controller.height = crouchHeight;
        }
        else {
            controller.height = normalHeight;
        }
    }
}
