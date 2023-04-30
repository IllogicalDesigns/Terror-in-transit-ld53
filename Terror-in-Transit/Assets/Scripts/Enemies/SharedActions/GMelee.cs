using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GMelee : GAction {
    private Transform target;
    private TrackedTarget trackedTarget;

    [SerializeField] private GameObject hurtBox;

    [SerializeField] private float delayToHurt = 0.5f;
    [SerializeField] private float boxLife = 0.25f;

    public override void Interruppted() {
        hurtBox.SetActive(false);
        StopAllCoroutines();
    }

    public override IEnumerator Perform() {
        gAgent.agent.isStopped = true;
        gameObject.SendMessage("PlayAttack");
        yield return new WaitForSeconds(delayToHurt);
        hurtBox.SetActive(true);
        yield return new WaitForSeconds(boxLife);
        hurtBox.SetActive(false);
        BlacklistAction();
        CompletedAction();
    }

    public override bool PostPerform() {
        hurtBox.SetActive(false);
        return true;
    }

    public override bool PrePerform() {
        gAgent.agentState.SetState("AgressionLevel", GGhost.Agression.agressive);
        gameObject.SendMessage("SetLightColor", LightColor.LightAwareness.hostile, SendMessageOptions.DontRequireReceiver);
        return true;
    }

    public override bool IsAchievable() {
        if (!gAgent.agentState.hasState("CurrentTarget")) return false;
        trackedTarget = gAgent.agentState.states["CurrentTarget"] as TrackedTarget;

        if (trackedTarget.detectionType != TrackedTarget.DetectionType.Visual) return false;
        if (trackedTarget.awareness < 2) return false;

        return base.IsAchievable();
    }

    // Start is called before the first frame update
    private void Start() {
        AddPreconditions("CurrentTarget");

        AddPreconditions("inMeleeRange");

        AddEffects("Attack");
    }

    // Update is called once per frame
    private void Update() {
        if (running && !gAgent.agentState.hasState("inMeleeRange")) {
            gAgent.AddGoal("SearchChase", 6, true);
            gAgent.Replan();
        }
    }
}
