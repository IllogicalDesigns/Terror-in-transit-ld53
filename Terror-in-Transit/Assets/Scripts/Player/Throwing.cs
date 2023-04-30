using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throwing : MonoBehaviour {
    [SerializeField] private LineRenderer LineRenderer;

    [SerializeField] [Range(10, 100)] private int LinePoints = 25;

    [SerializeField] [Range(0.01f, 0.25f)] private float TimeBetweenPoints = 0.1f;

    [SerializeField] private Transform ReleaseTransform;

    [SerializeField] private float ThrowStrength = 100f;

    [SerializeField] private float mass = 10f;

    [SerializeField] private LayerMask collisionMask;

    private Camera cam;

    [SerializeField] private HearableSound endOfProjection;

    private bool isEnable = false;

    [SerializeField] public int ammo = 0;
    [SerializeField] private int maxAmmo = 1;

    // Start is called before the first frame update
    private void Start() {
        cam = Camera.main;
    }

    // Update is called once per frame
    private void Update() {
        if (isEnable && ammo > 0) {
            HandleDrawingProjectionAndThrowing();

            if (Input.GetKeyDown(KeyCode.Alpha1)) isEnable = false;
        }
        else {
            LineRenderer.enabled = false;

            if (Input.GetKeyDown(KeyCode.Alpha1)) isEnable = true;
        }
    }

    public void AddAmmo(int i) {
        ammo += i;
        if (ammo > maxAmmo) ammo = maxAmmo;
    }

    private void HandleDrawingProjectionAndThrowing() {
        DrawProjection();

        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            ammo--;
            isEnable = false;

            var endPosition = LineRenderer.GetPosition(LineRenderer.positionCount - 1);
            endOfProjection.transform.position = endPosition;
            endOfProjection.EmitSound();
        }
    }

    private void DrawProjection() {
        LineRenderer.enabled = true;
        LineRenderer.positionCount = Mathf.CeilToInt(LinePoints / TimeBetweenPoints) + 1;
        Vector3 startPosition = ReleaseTransform.position;
        Vector3 startVelocity = ThrowStrength * ReleaseTransform.forward / mass;
        int i = 0;
        LineRenderer.SetPosition(i, startPosition);
        for (float time = 0; time < LinePoints; time += TimeBetweenPoints) {
            i++;
            Vector3 point = startPosition + time * startVelocity;
            point.y = startPosition.y + startVelocity.y * time + (Physics.gravity.y / 2f * time * time);

            LineRenderer.SetPosition(i, point);

            Vector3 lastPosition = LineRenderer.GetPosition(i - 1);

            if (Physics.Raycast(lastPosition, (point - lastPosition).normalized, out RaycastHit hit, (point - lastPosition).magnitude, collisionMask)) {
                LineRenderer.SetPosition(i, hit.point);
                LineRenderer.positionCount = i + 1;
                return;
            }
        }
    }
}
