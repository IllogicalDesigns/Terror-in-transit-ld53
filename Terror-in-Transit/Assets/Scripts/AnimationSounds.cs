using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSounds : MonoBehaviour {
    [SerializeField] private AudioSource footstepSFX;
    [SerializeField] private float footstepPitchVariance = 0.1f;
    [SerializeField] private float footstepStartingPitch = 1;

    // Start is called before the first frame update
    private void Start() {
    }

    // Update is called once per frame
    private void Update() {
    }

    public void PlayFootstep() {
        footstepSFX.pitch = footstepStartingPitch + Random.Range(-footstepPitchVariance, footstepPitchVariance);
        footstepSFX.Play();
    }
}
