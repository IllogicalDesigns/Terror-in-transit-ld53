using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightColor : MonoBehaviour {
    [SerializeField] private Color unawareColor = Color.white;
    [SerializeField] private Color awareColor = Color.yellow;
    [SerializeField] private Color hostileColor = Color.red;
    [SerializeField] private Light lightToColor;

    public enum LightAwareness {
        unaware,
        aware,
        hostile
    }

    private void Start() {
        SetLightColor(LightAwareness.unaware);
    }

    public void DisableLightFor(float duration) {
        lightToColor.gameObject.SetActive(false);
        Invoke("EnableLight", duration);
    }

    public void EnableLight() {
        lightToColor.gameObject.SetActive(true);
    }

    public void SetLightColor(LightAwareness awareness) {
        switch (awareness) {
            case LightAwareness.unaware:
                lightToColor.color = unawareColor;
                break;

            case LightAwareness.aware:
                lightToColor.color = awareColor;
                break;

            case LightAwareness.hostile:
                lightToColor.color = hostileColor;
                break;

            default:
                lightToColor.color = Color.magenta;
                break;
        }
    }
}
