using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GInvestigateSound : GAction {
    private Transform target;

    private bool isPerforming = false;

    [SerializeField] private float waitAtInvestigation = 1f;

    public override void Interruppted() {
    }

    public override IEnumerator Perform() {
        isPerforming = true;
        yield return AgentHelpers.GoToPosition(gAgent.agent, target.position);
        gAgent.agent.isStopped = true;
        yield return new WaitForSeconds(waitAtInvestigation);
        isPerforming = false;
        CompletedAction();
    }

    public override bool PostPerform() {
        gAgent.agentState.RemoveState("HeardTarget");
        return true;
    }

    public override bool PrePerform() {
        if (!gAgent.agentState.hasState("CurrentTarget")) return false;

        var tracked = gAgent.agentState.states["CurrentTarget"] as TrackedTarget;
        target = tracked.stimulator.transform;
        return true;
    }

    public override bool IsAchievable() {
        //if (!gAgent.agentState.hasState("playerAlertLevel")) return false;

        //var alertLevel = (GGhost.PlayerAlertLevel)gAgent.agentState.states["playerAlertLevel"];
        //if (alertLevel != GGhost.PlayerAlertLevel.Detected) return false;

        return base.IsAchievable();
    }

    // Start is called before the first frame update
    private void Start() {
        AddPreconditions("CurrentTarget");
        AddPreconditions("HeardTarget");
        //AddPreconditions("playerAlertLevel");

        AddEffects("Investigate");
    }

    // Update is called once per frame
    private void Update() {
        if (isPerforming && !gAgent.agentState.hasState("HeardTarget")) {
            gAgent.Replan();
        }
    }
}
