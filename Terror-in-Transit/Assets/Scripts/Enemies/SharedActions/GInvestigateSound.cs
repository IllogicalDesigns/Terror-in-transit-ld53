using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AI;

public class GInvestigate : GAction {
    private Transform target;
    private TrackedTarget trackedTarget;

    [SerializeField] private float waitAtInvestigation = 1f;
    [SerializeField] private float turnSpeed = 60;
    [SerializeField] private float agentSpeed = 2f;
    [SerializeField] private float closeDist = 1f;

    public override void Interruppted() {
        StopAllCoroutines();
    }

    public override IEnumerator Perform() {
        //if (gAgent.isBehind(target.position)) yield return AgentHelpers.RotateToFaceTarget(gAgent.transform, target, turnSpeed);
        //yield return AgentHelpers.GoToLastTracked(gAgent.agent, trackedTarget, closeDist);

        gAgent.RemoveGoal("SearchChase");
        gAgent.RemoveGoal("Search");

        if (gAgent.isBehind(trackedTarget.rawPosition)) {
            float angleThreshold = 1f;
            float rotationTime = 2f;

            gAgent.agent.isStopped = true;

            Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
            Quaternion startRotation = transform.rotation;
            float elapsedTime = 0f;

            while (elapsedTime < rotationTime && Quaternion.Angle(transform.rotation, targetRotation) > angleThreshold) {
                Quaternion newRotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / rotationTime);
                transform.rotation = newRotation;
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        do {
            gAgent.agent.isStopped = false;
            gAgent.agent.SetDestination(trackedTarget.rawPosition);
            yield return new WaitForSeconds(0.1f);
            //} while (Vector3.Distance(transform.position, new Vector3(trackedTarget.rawPosition.x, transform.position.y, trackedTarget.rawPosition.z)) > closeDist);
        } while (DistanceOnNavMesh(trackedTarget.rawPosition) > closeDist);

        yield return new WaitForSeconds(waitAtInvestigation);

        CompletedAction();
    }

    private float DistanceOnNavMesh(Vector3 source) {
        float totalDist = 0f;
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, source, NavMesh.AllAreas, path);

        for (int i = 0; i < path.corners.Length - 1; i++) {
            totalDist += Vector3.Distance(path.corners[i], path.corners[i + 1]);
            Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.cyan, 5f);
        }

        return totalDist;
    }

    public override bool PostPerform() {
        gAgent.agentState.RemoveState("HeardTarget");
        return true;
    }

    public override bool PrePerform() {
        if (!gAgent.agentState.hasState("CurrentTarget")) return false;

        var tracked = gAgent.agentState.states["CurrentTarget"] as TrackedTarget;
        target = tracked.stimulator.transform;

        if (target == null) return false;

        gAgent.agent.speed = agentSpeed;
        gameObject.SendMessage("SetLightColor", LightColor.LightAwareness.aware, SendMessageOptions.DontRequireReceiver);

        return true;
    }

    public override bool IsAchievable() {
        if (!gAgent.agentState.hasState("CurrentTarget")) return false;
        trackedTarget = gAgent.agentState.states["CurrentTarget"] as TrackedTarget;
        if (trackedTarget.awareness == 2) return false;
        if (trackedTarget.awareness < 0.75f) return false;

        return base.IsAchievable();
    }

    // Start is called before the first frame update
    private void Start() {
        AddPreconditions("CurrentTarget");

        AddEffects("Investigate");
    }

    // Update is called once per frame
    private void Update() {
    }
}
