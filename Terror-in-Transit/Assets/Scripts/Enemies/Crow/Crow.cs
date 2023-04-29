using DG.Tweening;
using UnityEngine;

public class LookAtTarget : MonoBehaviour {
    public Transform target1;
    public Transform target2;
    private Transform currentTarget;
    public float duration = 1f;

    private Ease ease = Ease.InOutSine;

    private void Start() {
        LookBackAndForth();
    }

    private void LookBackAndForth() {
        if (currentTarget == target1) currentTarget = target2;
        else currentTarget = target1;

        // Rotate the GameObject to look at target1, then jump to target2, and finally look back at target1
        transform.DOLookAt(currentTarget.position, duration).SetEase(ease).OnComplete(() => {
            LookBackAndForth();
        });
    }
}
