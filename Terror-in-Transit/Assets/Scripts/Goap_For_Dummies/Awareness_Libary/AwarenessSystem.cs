using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;

public class AwarenessSystem : MonoBehaviour {
    [SerializeField] private Dictionary<GameObject, TrackedTarget> targets = new Dictionary<GameObject, TrackedTarget>();

#if UNITY_EDITOR
    [SerializeField] public List<TrackedTarget> trackedObjectsDebug = new List<TrackedTarget>();
#endif // UNITY_EDITOR

    [Header("Vision Settings")]
    [SerializeField] private AnimationCurve visionSensitivity;

    [SerializeField] private float visionMinimumAwareness = 0f;
    [SerializeField] private float visionAwarenessBuildRate = 1f;

    [Header("Hearing Settings")]
    [SerializeField] private AnimationCurve hearingSensitivity;

    [SerializeField] private float hearingMinimumAwareness = 0f;

    [SerializeField] private float hearingAwarenessBuildRate = 1f;

    [Header("Proximity Settings (Unused)")]
    //[SerializeField] AnimationCurve proximitySensitivity;
    [SerializeField] private float proximityMinimumAwareness = 0f;

    [SerializeField] private float proximityAwarenessBuildRate = 1f;

    [Header("Decay Settings")]
    [SerializeField] private float awarenessDecayDelay = 0.1f;

    [SerializeField] private float awarenessDecayRate = 0.1f;

    private VisionSensor visionSensor;

    private GAgent gAgent;

    [SerializeField] private bool isDebug;

    public const float SUSMINIMUM = 0;
    public const float DECTMINIMUM = 1;
    public const float FULLMINIMUM = 2;

    public List<TrackedTarget> GetTargets() {
        var gameObjs = new List<TrackedTarget>();
        foreach (var item in targets) {
            gameObjs.Add(item.Value);
        }

        return gameObjs;
    }

    // Start is called before the first frame update
    private void Start() {
        visionSensor = gameObject.GetComponent<VisionSensor>();
        gAgent = gameObject.GetComponent<GAgent>();
    }

    public TrackedTarget getTrackedGameobject(GameObject targetGameObj) {
        if (!targets.ContainsKey(targetGameObj)) return null;
        return targets[targetGameObj];
    }

    public TrackedTarget GetMostAwareTrackedTarget() {
        if (targets.Count == 0) return null;

        var mostAware = targets.Values
        .OrderByDescending(t => t.awareness)
        .FirstOrDefault();

        return mostAware;
    }

    private string awarenessToColor(float awareness) {
        if (awareness >= 2)
            return "red";

        if (awareness > 1 && awareness < 2)
            return "yellow";

        if (awareness <= 1)
            return "green";

        return "magenta";
    }

    // Update is called once per frame
    private void FixedUpdate() {
        trackedObjectsDebug.Clear();
        if (targets.Count > 0) trackedObjectsDebug.AddRange(targets.Values);
    }

    // Update is called once per frame
    private void Update() {
        List<GameObject> toCleanup = new List<GameObject>();
        foreach (var t in targets.Keys) {
            var target = targets[t];
            if (target.DecayAwareness(awarenessDecayDelay, awarenessDecayRate * Time.deltaTime)) {
                if (target.awareness <= 0f) {
                    OnFullyLost(target);
                    toCleanup.Add(t);
                }
                else {
                    //Debug.Log("Threshold changed for " + gameObject.name + " threshold: " + target.awareness);
                    if (target.awareness >= 1f) OnLostDetection(target);
                    else OnLostSuspicion(target);
                }
            }
        }

        //cleanup dull targets
        foreach (var t in toCleanup) {
            targets.Remove(t);
        }
    }

    private void UpdateAwareness(GameObject targetGameObj, DetectableTarget target, Vector3 position, float awarenessContribution, float minimumAwareness, int priority = 1, TrackedTarget.DetectionType detectionType = TrackedTarget.DetectionType.Auditory) {
        //new object, add it
        if (targets.Count == 0 || !targets.ContainsKey(targetGameObj)) {
            targets[targetGameObj] = new TrackedTarget(targetGameObj, target, position, Time.time, minimumAwareness, priority, detectionType);
        }
        else {
            targets[targetGameObj].detectionType = detectionType;
            //Update target awareness
            if (targets[targetGameObj].UpdateAwareness(target, position, awarenessContribution, minimumAwareness)) {  //TODO remove
                // Debug.Log("Threshold changed for " + gameObject.name + " threshold: " + targets[targetGameObj].awareness);
            }
        }

        var awareObject = targets[targetGameObj];

        if (awareObject.awareness >= 2f)
            OnFullyDetected(awareObject);
        else if (awareObject.awareness >= 1f)
            OnDetected(awareObject);
        else
            OnSuspicious(awareObject);

#if UNITY_EDITOR
        //if (targets != null) {
        //}
#endif // UNITY_EDITOR
    }

    public void ReportInProximity(DetectableTarget seen) {
        var awarenessContribution = hearingAwarenessBuildRate * Time.deltaTime;

        UpdateAwareness(seen.gameObject, seen, seen.transform.position, awarenessContribution, proximityMinimumAwareness, 1, TrackedTarget.DetectionType.Proximity);
    }

    public void ReportCanSee(DetectableTarget seen, float multi = 1f) {
        if (!visionSensor) return;

        // determine where in the field of view the target is at
        var vectorToTarget = (seen.transform.position - visionSensor.eyeLocation).normalized;
        var dotProduct = Vector3.Dot(vectorToTarget, visionSensor.eyeDirection);

        // determine the awareness contribution this frame (stimulation)
        var awarenessContribution = visionSensitivity.Evaluate(dotProduct) * visionAwarenessBuildRate * multi * Time.deltaTime;

        if (isDebug) Debug.Log(gameObject.name + " has Seen " + seen.gameObject.name);

        UpdateAwareness(seen.gameObject, seen, seen.transform.position, awarenessContribution, visionMinimumAwareness, 1, TrackedTarget.DetectionType.Visual);
    }

    public void ReportCanHear(GameObject source, HearingManager.EHeardSoundCategory heardSoundCategory, float intensity) {
        var awarenessContribution = intensity * hearingAwarenessBuildRate * Time.deltaTime;

        if (isDebug) Debug.Log(gameObject.name + " has heard " + source.name + "[" + heardSoundCategory.ToString() + ", intensity:" + intensity);

        UpdateAwareness(source.gameObject, null, source.transform.position, awarenessContribution, hearingMinimumAwareness, 1, TrackedTarget.DetectionType.Auditory);
    }

    public bool CanSee(GameObject trackedTarget) {
        var seeFloat = visionSensor.CanWeSeeTarget(trackedTarget);
        return seeFloat <= 0f ? false : true;
    }

    public void OnSuspicious(TrackedTarget target) {
        if (isDebug) Debug.Log(gameObject.name + " OnSuspicious(" + target.stimulator.name + ")");
        gAgent.OnSuspicious(target);
    }

    public void OnLostSuspicion(TrackedTarget target) {
        if (isDebug) Debug.Log(gameObject.name + " OnLostSuspicion(" + target.stimulator.name + ")");
        gAgent.OnLostSuspicion(target);
    }

    public void OnDetected(TrackedTarget target) {
        if (isDebug) Debug.Log(gameObject.name + " OnDetected(" + target.stimulator.name + ")");
        gAgent.OnDetected(target);
    }

    public void OnFullyDetected(TrackedTarget target) {
        if (isDebug) Debug.Log(gameObject.name + " OnFullyDetected(" + target.stimulator.name + ")");
        gAgent.OnFullyDetected(target);
    }

    public void OnLostDetection(TrackedTarget target) {
        if (isDebug) Debug.Log(gameObject.name + " OnLostDetection(" + target.stimulator.name + ")");
        gAgent.OnLostDetection(target);
    }

    public void OnFullyLost(TrackedTarget target) {
        if (isDebug) Debug.Log(gameObject.name + " OnFullyLost(" + target.stimulator.name + ")");
        gAgent.OnFullyLost(target);
    }
}

[Serializable]
public class TrackedTarget {
    public GameObject stimulator;
    public DetectableTarget detectable;
    public Vector3 rawPosition;

    public Vector3 lastPosition;
    public Vector3 lastDirection;

    public float LastSensedTime;
    public float awareness;  //0 is unaware (to be culled), 0-1 rough awareness, 1-2 likely aware, 2+ full awareness

    public float awarenessContributionRate;

    public int priority = 0;

    public enum DetectionType { Auditory, Visual, Proximity };

    public DetectionType detectionType;

    public TrackedTarget(GameObject _stimulator, DetectableTarget _detectable, Vector3 _rawPosition, float _lastSensedTime, float _awareness, int _priority = 1, DetectionType _detectionType = DetectionType.Auditory) {
        this.stimulator = _stimulator;
        this.detectable = _detectable;
        this.rawPosition = _rawPosition;
        LastSensedTime = _lastSensedTime;
        this.awareness = _awareness;
        this.priority = _priority;
        this.detectionType = _detectionType;
    }

    public bool UpdateAwareness(DetectableTarget target, Vector3 position, float awarenessContribution, float minimumAwareness) {
        var oldAwareness = awareness;

        if (target) {
            detectable = target;
            lastPosition = target.transform.position;
            lastDirection = target.transform.forward;
        }

        rawPosition = position;
        LastSensedTime = Time.time;

        awareness = Mathf.Clamp(awareness + awarenessContribution, 0f, 2f);

        awarenessContributionRate = awarenessContribution;

        if (oldAwareness < 2f && awareness >= 2f)  //crossed from threating to agressive
            return true;

        if (oldAwareness < 1f && awareness >= 1f)  //crossed from interesting to threating
            return true;

        if (oldAwareness <= 0f && awareness >= 0f)  //crossed from nothing to interesting
            return true;

        return false;
    }

    public bool DecayAwareness(float decayDelay, float amount) {
        //detected too recently
        if ((Time.time - LastSensedTime) < decayDelay) return false;

        var oldAwareness = awareness;

        awareness -= amount;

        if (oldAwareness >= 2f && awareness < 2f)  //crossed from threating to agressive
            return true;

        if (oldAwareness >= 1f && awareness < 1f)  //crossed from interesting to threating
            return true;

        return awareness <= 0f;
    }
}
