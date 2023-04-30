using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class DroneManager : MonoBehaviour {
    private bool isInDrone = false;
    [SerializeField] private UnityEvent onDiablePlayer;
    [SerializeField] private UnityEvent onEnablePlayer;

    [SerializeField] private UnityEvent onDiableDrone;
    [SerializeField] private UnityEvent onEnableDrone;

    [SerializeField] private Transform player;

    private float y;

    private Tween tween;

    [SerializeField] private float speed = 5f;
    [SerializeField] private float droneMaxDist = 15f;

    // Start is called before the first frame update
    private void Start() {
        y = transform.position.y;
    }

    // Update is called once per frame
    private void Update() {
        if (Input.GetKeyDown(KeyCode.M)) {
            isInDrone = !isInDrone;

            if (isInDrone) {
                onEnableDrone.Invoke();
                onDiablePlayer.Invoke();
            }
            else {
                onEnablePlayer.Invoke();
                onDiableDrone.Invoke();
            }
        }

        if (tween == null && !isInDrone)
            tween = transform.DOMove(new Vector3(player.transform.position.x, y, player.transform.position.z), 1f).OnComplete(() => { tween = null; });

        if (isInDrone) {
            var v = Input.GetAxis("Vertical");
            var h = Input.GetAxis("Horizontal");

            var spd = speed;
            if (!(Vector3.Distance(new Vector3(player.transform.position.x, y, player.transform.position.z), transform.position) < droneMaxDist)) spd = 1f;

            var timeSpeed = spd * Time.deltaTime;
            transform.Translate(new Vector3(timeSpeed * h, 0, timeSpeed * v));
        }
    }
}
