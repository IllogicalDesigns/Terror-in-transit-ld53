using DG.Tweening;
using UnityEngine;

public class LookAtTarget : MonoBehaviour {
    public Transform target1;
    public Transform target2;
    private Transform currentTarget;
    public float duration = 1f;

    private Ease ease = Ease.InOutSine;

    private Tween tween;
    public float stopDuration = 1f;
    private float timer = -1;

    private void Start() {
        LookBackAndForth();
    }

    public void StopLooking() {
        tween.Kill();

        timer = stopDuration;
    }

    private void Update() {
        if (timer > 0) timer -= Time.deltaTime;

        if (timer == 0) LookBackAndForth();
    }

    private void LookBackAndForth() {
        if (currentTarget == target1) currentTarget = target2;
        else currentTarget = target1;

        // Rotate the GameObject to look at target1, then jump to target2, and finally look back at target1
        tween = transform.DOLookAt(currentTarget.position, duration).SetEase(ease).OnComplete(() => {
            LookBackAndForth();
        });
    }
}
