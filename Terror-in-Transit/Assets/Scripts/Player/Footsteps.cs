using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footsteps : MonoBehaviour {
    [SerializeField] private PlayerMovement playerMovement;
    private float raycastDistance = 1f;

    [SerializeField] private AudioSource footSrc;

    public AudioClip[] concreteFootstepSounds;
    public AudioClip[] dirtFootstepSounds;
    public AudioClip[] grassFootstepSounds;

    private AudioClip defaultFootstep;

    [SerializeField] private HearableSound hearableSound;

    private float timer;
    private float footStepTime = 0.5f;

    public enum SurfaceType {
        Concrete,
        Dirt,
        Grass
    }

    // Start is called before the first frame update
    private void Start() {
    }

    // Update is called once per frame
    private void Update() {
        if (timer > 0) timer -= Time.deltaTime;

        if (playerMovement.currentSpeed > 0 && timer <= 0) {
            RaycastHit hit;

            // Perform the raycast downwards
            if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, raycastDistance)) {
                Debug.DrawLine(transform.position + Vector3.up, hit.point, Color.cyan, 5f);
                PlayFootStep(SurfaceType.Grass);
            }

            timer = footStepTime;
        }
    }

    private void PlayFootStep(SurfaceType currentSurfaceType) {
        if (footSrc == null) return;

        AudioClip[] footstepSounds;
        bool hearable = false;

        switch (currentSurfaceType) {
            case SurfaceType.Concrete:
                footstepSounds = concreteFootstepSounds;
                break;

            case SurfaceType.Dirt:
                footstepSounds = dirtFootstepSounds;
                break;

            case SurfaceType.Grass:
                footstepSounds = grassFootstepSounds;
                break;

            default:
                footstepSounds = concreteFootstepSounds;
                break;
        }

        int speed = 0;
        switch (playerMovement.movementState) {
            case PlayerMovement.MovementState.Crouching:
                speed = 0;
                break;

            case PlayerMovement.MovementState.Sprinting:
                speed = 2;
                break;

            default:
                speed = 1;
                break;
        }

        AudioClip footstepSound;

        // Choose a random footstep sound from the appropriate array
        if (playerMovement.movementState == PlayerMovement.MovementState.Sprinting) {
            hearableSound.EmitSound();
        }

        if (footstepSounds.Length == 0) footstepSound = defaultFootstep;
        else
            footstepSound = footstepSounds[speed];

        if (hearable && hearableSound != null) hearableSound.EmitSound();

        // Play the footstep sound using the AudioSource component
        footSrc.PlayOneShot(footstepSound);
    }
}
