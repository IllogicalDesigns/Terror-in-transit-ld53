using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GMelee : GAction {
    private Transform target;

    private bool isPerforming = false;

    [SerializeField] private GameObject hurtBox;

    public override void Interruppted() {
    }

    public override IEnumerator Perform() {
        gAgent.agent.isStopped = true;
        hurtBox.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        hurtBox.SetActive(false);
        BlacklistAction();
        CompletedAction();
    }

    public override bool PostPerform() {
        return true;
    }

    public override bool PrePerform() {
        return true;
    }

    public override bool IsAchievable() {
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
        AddPreconditions("inMeleeRange");

        AddEffects("Attack");
    }

    // Update is called once per frame
    private void Update() {
        if (isPerforming && !gAgent.agentState.hasState("VisualOnTarget")) {
            gAgent.Replan();
        }

        //if (timer > 0) timer -= Time.deltaTime;
    }
}
