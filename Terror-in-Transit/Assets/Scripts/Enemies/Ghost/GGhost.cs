using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GGhost : GAgent {

    public enum PlayerAlertLevel {
        None,
        Suspicious,
        Detected,
        FullyDetected
    }

    public enum Agression {
        bored,
        annoyed,
        agressive
    }

    [SerializeField] private AwarenessSystem awarenessSystem;

    [SerializeField] private TrackedTarget currentTarget;

    [SerializeField] private float meleeDist = 3f;

    // Start is called before the first frame update
    public override void Start() {
        base.Start();

        AddGoal("Attack", 2, false);
        AddGoal("Chase", 3, false);
        //AddGoal("Investigate", 5, false);
        AddGoal("Patrol", 10, false);

        agentState.SetState("AgressionLevel", Agression.bored);
    }

    private void Update() {
        HandleVisualState();
        HandleMeleeRange();

        if (currentTarget != null && currentTarget.awareness <= 0) {
            agentState.RemoveState("CurrentTarget");
            currentTarget = null;
        }
    }

    public void Stun() {
        AddGoal("Stunned", 1, true);
        Replan();
    }

    // Update is called once per frame
    private void FixedUpdate() {
    }

    private void HandleMeleeRange() {
        if (currentTarget == null || currentTarget.stimulator == null) {
            agentState.RemoveState("inMeleeRange");
            return;
        }

        var dist = Vector3.Distance(transform.position, currentTarget.stimulator.transform.position);
        if (dist > meleeDist)
            agentState.RemoveState("inMeleeRange");
        else if (!agentState.hasState("inMeleeRange")) {
            agentState.SetState("inMeleeRange");
            Replan();
        }
    }

    private static bool IsPlayer(TrackedTarget target) {
        return target.stimulator.name == "Player";
    }

    private void SetPlayerAlertLevel(TrackedTarget target, PlayerAlertLevel alertLevel) {
        if (IsPlayer(target)) {
            agentState.SetState("playerAlertLevel", alertLevel);
            agentState.SetState("CurrentTarget", target);
            currentTarget = target;
        }
    }

    private void HandleVisualState() {
        if (currentTarget == null) {
            return;
        }
        else if (currentTarget.stimulator == null) {
            currentTarget.detectionType = TrackedTarget.DetectionType.Auditory;
            return;
        }

        if (awarenessSystem.CanSee(currentTarget.stimulator))
            currentTarget.detectionType = TrackedTarget.DetectionType.Visual;
        else if (currentTarget.detectionType == TrackedTarget.DetectionType.Auditory)
            currentTarget.detectionType = TrackedTarget.DetectionType.Auditory;
        else
            currentTarget.detectionType = TrackedTarget.DetectionType.None;
    }

    private bool atAlertState(PlayerAlertLevel alertState) {
        if (!agentState.hasState("playerAlertLevel")) return false;

        var alertLevel = (PlayerAlertLevel)agentState.states["playerAlertLevel"];
        if (alertLevel != alertState) return false;

        return true;
    }

    private void SetBetterCurrentTarget(TrackedTarget target) {
        if (currentTarget == null) {
            SetCurrentTarget(target);
            return;
        }

        var bestTarget = awarenessSystem.GetMostAwareTrackedTarget();
        SetCurrentTarget(bestTarget);

        void SetCurrentTarget(TrackedTarget target) {
            if (target.stimulator != null) Debug.Log("New target " + target.stimulator.name);
            else Debug.Log("New target " + "Does not have a stimulator to be named");

            AddGoal("Investigate", 5, true);
            agentState.SetState("CurrentTarget", target);
            currentTarget = target;
            Replan();
        }
    }

    public override void OnSuspicious(TrackedTarget target) {
        //SetBetterCurrentTarget(target);
    }

    public override void OnLostSuspicion(TrackedTarget target) {
    }

    public override void OnDetected(TrackedTarget target) {
        SetBetterCurrentTarget(target);
    }

    public override void OnLostDetection(TrackedTarget target) {
    }

    public override void OnFullyDetected(TrackedTarget target) {
        SetBetterCurrentTarget(target);
    }

    public override void OnFullyLost(TrackedTarget target) {
    }
}
