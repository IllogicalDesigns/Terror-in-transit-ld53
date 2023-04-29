using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HearableSound : MonoBehaviour {
    [SerializeField] private HearingManager.EHeardSoundCategory heardSoundCategory = HearingManager.EHeardSoundCategory.EImportant;
    [SerializeField] private float intensity = 1f;
    [SerializeField] private float overrideHearingRange = 0f;

    [SerializeField] private AudioSource audioSource;

    // Start is called before the first frame update
    private void Start() {
    }

    // Update is called once per frame
    private void Update() {
    }

    public void EmitSound() {
        if (audioSource) audioSource.Play();

        HearingSensor[] ears = (HearingSensor[])FindObjectsOfType(typeof(HearingSensor));
        foreach (var ear in ears) {
            ear.OnHeardSound(this.gameObject, heardSoundCategory, intensity, overrideHearingRange);
        }
    }
}
