using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GChase : GAction {
    private Transform target;

    private bool isPerforming = false;

    public override void Interruppted() {
    }

    public override IEnumerator Perform() {
        isPerforming = true;
        yield return AgentHelpers.GoToPosition(gAgent.agent, target.position);
        isPerforming = false;
        CompletedAction();
    }

    public override bool PostPerform() {
        return true;
    }

    public override bool PrePerform() {
        if (!gAgent.agentState.hasState("CurrentTarget")) return false;

        var tracked = gAgent.agentState.states["CurrentTarget"] as TrackedTarget;
        target = tracked.stimulator.transform;
        return true;
    }

    public override bool IsAchievable() {
        //if (!gAgent.agentState.hasState("CurrentTarget")) return false;
        //if (!gAgent.agentState.hasState("VisualOnTarget")) return false;
        if (!gAgent.agentState.hasState("playerAlertLevel")) return false;

        var alertLevel = (GGhost.PlayerAlertLevel)gAgent.agentState.states["playerAlertLevel"];
        if (alertLevel != GGhost.PlayerAlertLevel.FullyDetected) return false;

        return base.IsAchievable();
    }

    // Start is called before the first frame update
    private void Start() {
        AddPreconditions("CurrentTarget");
        AddPreconditions("VisualOnTarget");
        AddPreconditions("playerAlertLevel");

        AddEffects("Chase");
    }

    // Update is called once per frame
    private void Update() {
        if (isPerforming && !gAgent.agentState.hasState("VisualOnTarget")) {
            gAgent.Replan();
        }
    }
}
