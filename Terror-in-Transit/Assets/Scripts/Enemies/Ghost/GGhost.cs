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
        AddGoal("Investigate", 4, false);
        AddGoal("Patrol", 5, false);

        agentState.SetState("AgressionLevel", Agression.bored);
    }

    private void Update() {
        HandleVisualState();
        HandleMeleeRange();
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
        if (currentTarget == null || currentTarget.detectionType != TrackedTarget.DetectionType.Visual) {
            agentState.RemoveState("VisualOnTarget");
            return;
        }

        if (awarenessSystem.CanSee(currentTarget.stimulator))
            agentState.SetState("VisualOnTarget", true);
        else
            agentState.RemoveState("VisualOnTarget");
    }

    private bool atAlertState(PlayerAlertLevel alertState) {
        if (!agentState.hasState("playerAlertLevel")) return false;

        var alertLevel = (PlayerAlertLevel)agentState.states["playerAlertLevel"];
        if (alertLevel != alertState) return false;

        return true;
    }

    public override void OnSuspicious(TrackedTarget target) {
        if (currentTarget == target && atAlertState(PlayerAlertLevel.Suspicious)) return;
        SetPlayerAlertLevel(target, PlayerAlertLevel.Suspicious);

        if (target.detectionType == TrackedTarget.DetectionType.Auditory) agentState.SetState("HeardTarget");
        SetBetterCurrentTarget(target);
    }

    private void SetBetterCurrentTarget(TrackedTarget target) {
        if (currentTarget == null || target.awareness > currentTarget.awareness) {
            if (agentState.hasState("HeardTarget") && target.detectionType != TrackedTarget.DetectionType.Auditory) agentState.RemoveState("HeardTarget");
            agentState.SetState("CurrentTarget", target);
            Replan();
        }
    }

    public override void OnLostSuspicion(TrackedTarget target) {
        if (currentTarget == target && atAlertState(PlayerAlertLevel.Suspicious)) return;
        SetPlayerAlertLevel(target, PlayerAlertLevel.None);
    }

    public override void OnDetected(TrackedTarget target) {
        if (currentTarget == target && atAlertState(PlayerAlertLevel.Detected)) return;
        SetPlayerAlertLevel(target, PlayerAlertLevel.Detected);
        if (target.detectionType == TrackedTarget.DetectionType.Auditory) agentState.SetState("HeardTarget");
        SetBetterCurrentTarget(target);
    }

    public override void OnLostDetection(TrackedTarget target) {
        if (currentTarget == target && atAlertState(PlayerAlertLevel.Detected)) return;
        SetPlayerAlertLevel(target, PlayerAlertLevel.Suspicious);
    }

    public override void OnFullyDetected(TrackedTarget target) {
        if (currentTarget == target && atAlertState(PlayerAlertLevel.FullyDetected)) return;
        SetPlayerAlertLevel(target, PlayerAlertLevel.FullyDetected);
        if (target.detectionType == TrackedTarget.DetectionType.Auditory) agentState.SetState("HeardTarget");
        SetBetterCurrentTarget(target);

        if (IsPlayer(target)) Replan();
    }

    public override void OnFullyLost(TrackedTarget target) {
        agentState.RemoveState("playerAlertLevel");
        if (currentTarget == target) agentState.RemoveState("CurrentTarget");
    }
}
