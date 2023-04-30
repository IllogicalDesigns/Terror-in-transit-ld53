using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetScaleEachFrame : MonoBehaviour {
    private bool isFlippedX = true;
    private bool isFlippedY = true;
    private bool isFlippedZ = true;

    // Start is called before the first frame update
    private void Start() {
    }

    // Update is called once per frame
    private void Update() {
        transform.localScale = new Vector3(isFlippedX ? 1 : -1, isFlippedY ? 1 : -1, isFlippedZ ? 1 : -1);
        if (Random.Range(0, 100) > 75) isFlippedX = !isFlippedX;
        if (Random.Range(0, 100) > 75) isFlippedY = !isFlippedY;
        if (Random.Range(0, 100) > 75) isFlippedZ = !isFlippedZ;
    }
}
