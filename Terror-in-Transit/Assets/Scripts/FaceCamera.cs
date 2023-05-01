using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FaceCamera : MonoBehaviour {
    private Camera cam;
    private Tween tween;
    private float duration = 1f;

    // Start is called before the first frame update
    private void Start() {
        cam = Camera.main;
    }

    // Update is called once per frame
    private void Update() {
        if (tween == null)
            tween = transform.DOLookAt(cam.transform.position, duration).OnComplete(() => { tween = null; });
    }
}
