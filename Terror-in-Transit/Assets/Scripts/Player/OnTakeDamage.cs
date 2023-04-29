using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using DG.Tweening;

public class OnTakeDamage : MonoBehaviour {
    [SerializeField] private CinemachineImpulseSource hurtSource;
    [SerializeField] private AudioSource hurtSrc;
    [SerializeField] private Image vignette;
    [SerializeField] private float vignetteTime = 1f;

    // Start is called before the first frame update
    private void Start() {
    }

    // Update is called once per frame
    private void Update() {
    }

    public void disableVignette() {
        vignette.enabled = false;
    }

    public void ApplyDamage(int damage) {
        hurtSrc.Play();
        hurtSource.GenerateImpulse();
        vignette.DOFade(0.5f, 0.05f).OnComplete(() => {
            vignette.DOFade(0f, vignetteTime);
        });
    }
}
