using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HearingManager : MonoBehaviour {
    public static HearingManager Instance { get; private set; } = null;  // read-only, internally setable

    public List<HearingSensor> AllSensors = new List<HearingSensor>();

    public enum EHeardSoundCategory {
        EFootStep,
        EJump,
        EGunShot,
        EImportant
    }

    private void Awake() {
        if (Instance) {
            Debug.LogError("Multiple hearing manager instances, it should be a singleton, destroying " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Start is called before the first frame update
    private void Start() {
    }

    // Update is called once per frame
    private void Update() {
    }

    public void Register(HearingSensor sensor) {
        AllSensors.Add(sensor);
    }

    public void DeRegister(HearingSensor sensor) {
        AllSensors.Remove(sensor);
    }

    public void OnSoundEmitted(GameObject source, HearingManager.EHeardSoundCategory category, float intensity) {
        foreach (var sensor in AllSensors) {
            sensor.OnHeardSound(source, category, intensity);
        }
    }
}
