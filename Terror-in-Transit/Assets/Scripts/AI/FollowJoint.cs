using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowJoint : MonoBehaviour {
    private Vector3 offset = Vector3.zero;
    [SerializeField] private Transform joint;
    [SerializeField] private Transform parent;

    // Start is called before the first frame update
    private void Start() {
    }

    // Update is called once per frame
    private void Update() {
        transform.position = joint.transform.position + offset;
        transform.forward = parent.forward;
    }
}
