using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GChase : GAction {
    private Transform target;
    private TrackedTarget trackedTarget;
    [SerializeField] private float agentSpeed = 3.5f;

    [SerializeField] private float closeDist = 2f;

    public override void Interruppted() {
        StopAllCoroutines();
    }

    public override IEnumerator Perform() {
        gAgent.RemoveGoal("SearchChase");
        gAgent.RemoveGoal("Search");

        gameObject.SendMessage("BarkLine", "Spotted");

        Vector3 oldPos = transform.position;
        do {
            gAgent.agent.isStopped = false;
            gAgent.agent.SetDestination(trackedTarget.rawPosition);
            yield return new WaitForSeconds(0.01f);
            if (transform.position == oldPos) break;
            else oldPos = transform.position;
        } while (AgentHelpers.DistanceOnNavMesh(transform, trackedTarget.rawPosition) > closeDist);

        yield return new WaitForSeconds(1f);
        CompletedAction();
    }

    public override bool PostPerform() {
        return true;
    }

    public override bool PrePerform() {
        if (!gAgent.agentState.hasState("CurrentTarget")) return false;

        var tracked = gAgent.agentState.states["CurrentTarget"] as TrackedTarget;
        target = tracked.stimulator.transform;

        if (target == null) return false;

        gAgent.agentState.SetState("AgressionLevel", GGhost.Agression.agressive);

        gAgent.agent.speed = agentSpeed;
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

        AddEffects("Chase");
    }

    // Update is called once per frame
    private void Update() {
        if (running && trackedTarget.detectionType != TrackedTarget.DetectionType.Visual) {
            gAgent.AddGoal("SearchChase", 4, true);
            gAgent.Replan();
        }
    }
}
