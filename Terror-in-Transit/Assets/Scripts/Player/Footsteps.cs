using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Footsteps : MonoBehaviour {
    [SerializeField] private Vector3 offset = new Vector3(0, 1, 0);

    [SerializeField] private HearableSound footstepHearableSound;

    [Header("footstep Sfx")]
    [SerializeField] private AudioSource footstepSFX;

    [SerializeField] private float footstepPitchVariance = 0.1f;
    [SerializeField] private float footstepStartingPitch = 1;

    [Header("running footstep Sfx")]
    [SerializeField] private AudioSource runStepSFX;

    [SerializeField] private float runStepPitchVariance = 0.1f;
    [SerializeField] private float runStepStartingPitch = 1;

    [Header("Leaves footstep Sfx")]
    [SerializeField] private LayerMask layerMask;

    [SerializeField] private AudioSource wetFootstepSFX;

    [SerializeField] private float leavesFootstepPitchVariance = 0.1f;
    [SerializeField] private float leavesFootstepStartingPitch = 1;
    [SerializeField] private string leavesFootstep = "Leaves";

    private PlayerMovement playerMovement;
    [SerializeField] private float crouchstepTime = 0.75f;
    [SerializeField] private float footstepTime = 0.5f;
    [SerializeField] private float runstepTime = 0.25f;
    private float timer;

    [SerializeField] private Image noiseIcon;
    [SerializeField] private float noiseTime = 1f;

    // Start is called before the first frame update
    private void Start() {
        playerMovement = gameObject.GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    private void Update() {
        if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0) {
            if (timer > 0) {
                timer -= Time.deltaTime;
                return;
            }

            if (playerMovement.movementState == PlayerMovement.MovementState.Crouching) {
                PlayCrouchStepSFX();
            }
            else if (playerMovement.movementState == PlayerMovement.MovementState.Walking) {
                PlayFootstepSFX();
            }
            else {
                PlayRunstepSFX();
            }
        }
    }

    public void PlayJumpSound() {
        PlayRunstepSFX();
    }

    public void PlayLeavesSound() {
        ShowNoiseSymbol();

        footstepHearableSound.EmitSound();
        wetFootstepSFX.pitch = leavesFootstepStartingPitch + Random.Range(-leavesFootstepPitchVariance, leavesFootstepPitchVariance);
        wetFootstepSFX.Play();
    }

    public void ShowNoiseSymbol() {
        if (noiseIcon == null) {
            Debug.Log("Footsteps:ShowNoiseSymbol() noiseIcon is null");
            return;
        }

        noiseIcon.DOFade(0.5f, 0.05f).OnComplete(() => {
            noiseIcon.DOFade(0f, noiseTime);
        });
    }

    public void PlayCrouchStepSFX() {
        //Debug.DrawRay(transform.position + offset, Vector3.down * 100f, Color.cyan, 100f);

        RaycastHit hit;
        if (Physics.Raycast(transform.position + offset, Vector3.down, out hit, Mathf.Infinity, layerMask)) {
            PlayStepSound(hit);
        }

        void PlayStepSound(RaycastHit hit) {
            if (hit.collider.CompareTag(leavesFootstep))
                PlayLeavesSound();
            else {
                //footstepSFX.volume = 0.5f;
                footstepSFX.pitch = footstepStartingPitch + Random.Range(-footstepPitchVariance, footstepPitchVariance);
                footstepSFX.Play();
            }
        }

        timer = crouchstepTime;
    }

    public void PlayFootstepSFX() {
        //Debug.DrawRay(transform.position + offset, Vector3.down * 100f, Color.cyan, 100f);

        RaycastHit hit;
        if (Physics.Raycast(transform.position + offset, Vector3.down, out hit, Mathf.Infinity, layerMask)) {
            PlayStepSound(hit);
        }

        void PlayStepSound(RaycastHit hit) {
            if (hit.collider.CompareTag(leavesFootstep))
                PlayLeavesSound();
            else {
                //footstepSFX.volume = 1f;
                footstepSFX.pitch = footstepStartingPitch + Random.Range(-footstepPitchVariance, footstepPitchVariance);
                footstepSFX.Play();
            }
        }

        timer = footstepTime;
    }

    public void PlayRunstepSFX() {
        Debug.DrawRay(transform.position + offset, Vector3.down * 100f, Color.blue, 100f);

        RaycastHit hit;
        if (Physics.Raycast(transform.position + offset, Vector3.down, out hit, Mathf.Infinity, layerMask)) {
            PlayRunningSound(hit);
        }

        void PlayRunningSound(RaycastHit hit) {
            if (hit.collider.CompareTag(leavesFootstep))
                PlayLeavesSound();
            else {
                footstepHearableSound.EmitSound();
                runStepSFX.pitch = runStepStartingPitch + Random.Range(-runStepPitchVariance, runStepPitchVariance);
                runStepSFX.Play();

                ShowNoiseSymbol();
            }
        }

        timer = runstepTime;
    }
}
