using UnityEngine;

public class Leaning : MonoBehaviour {
    public GameObject left;
    public GameObject center;
    public GameObject right;

    private void Update() {
        // Check for left key press
        if (Input.GetKey(KeyCode.Q)) {
            left.SetActive(true);
            center.SetActive(false);
            right.SetActive(false);
        }

        // Check for right key press
        else if (Input.GetKey(KeyCode.E)) {
            left.SetActive(false);
            center.SetActive(false);
            right.SetActive(true);
        }

        // No key pressed, enable center
        else {
            left.SetActive(false);
            center.SetActive(true);
            right.SetActive(false);
        }
    }
}
