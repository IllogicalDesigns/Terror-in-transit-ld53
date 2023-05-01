using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class GSearchAfterChase : GAction {
    private Transform DetectionTransform;

    private InfluenceMap influenceMap;
    private TrackedTarget investigateTarget;

    [Header("Action specific settings")]
    [SerializeField] private int searchStesps = 20;

    public Vector3 chasePosition;

    public delegate void OnCoroutineFinished(Vector3 centroidPosition);

    private bool isWaiting = false;
    [SerializeField] private float speed = 3.5f, closeDist = 2f;

    public override void Awake() {
        base.Awake();

        influenceMap = FindObjectOfType<InfluenceMap>();

        //AddPreconditions("HostileTarget");
        //AddPreconditions("SeenPlayer");
        //AddEffects("Chase");

        //AddPreconditions("FullyDetectionTarget");
        AddEffects("SearchChase");
    }

    public override void Interruppted() {
    }

    public override bool IsAchievable() {
        //if (!gAgent.agentState.hasState("HostileTarget")) return false;
        return base.IsAchievable();
    }

    public void RecieveCentroidPositon(Vector3 centroidPosition) {
        isWaiting = false;

        centroidPosition = AgentHelpers.CheckAndNudgePosAll(centroidPosition);
        chasePosition = centroidPosition;
    }

    public override IEnumerator Perform() {
        gAgent.agent.speed = speed;

        gAgent.agent.SetDestination(DetectionTransform.position);  //keep moving to the last known well we calculate;
        gameObject.SendMessage("SetLightColor", LightColor.LightAwareness.aware, SendMessageOptions.DontRequireReceiver);

        isWaiting = true;
        Vector3 direction = DetectionTransform.position - investigateTarget.rawPosition;
        yield return influenceMap.HeatWaveChasePropagation(DetectionTransform, transform, RecieveCentroidPositon, searchStesps);

        do {
            yield return new WaitForSeconds(0.1f);
        } while (isWaiting);

        Vector3 oldPos = transform.position;
        do {
            gAgent.agent.isStopped = false;
            gAgent.agent.SetDestination(chasePosition);
            yield return new WaitForSeconds(0.01f);
            //} while (Vector3.Distance(transform.position, chasePosition) > closeDist);
            if (transform.position == oldPos) break;
            else oldPos = transform.position;
        } while (AgentHelpers.DistanceOnNavMesh(transform, chasePosition) > closeDist);

        //yield return AgentHelpers.GoToPosition(gAgent.agent, chasePosition, 2);

        CompletedAction();
    }

    public override bool PostPerform() {
        chasePosition = Vector3.zero;
        gAgent.AddGoal("Search", 6, true);

        //Bark here to get all AIs to search
        gameObject.SendMessage("BarkLine", "BroadSearch");

        var guards = FindObjectsOfType<GGhost>();

        bool confirmed = false;
        foreach (var guard in guards) {
            guard.AddGoal("Search", 6, true);
            guard.Replan();

            if (confirmed || guard.gameObject == gameObject) continue;
            guard.SendMessage("BarkLine", "Confirm");
            confirmed = true;
        }

        return true;
    }

    public override bool PrePerform() {
        investigateTarget = (TrackedTarget)gAgent.agentState.GetStates()["CurrentTarget"];
        if (investigateTarget == null) return false;
        if (investigateTarget.stimulator == null || investigateTarget.stimulator.transform == null) return false;

        DetectionTransform = investigateTarget.stimulator.transform;

        if (DetectionTransform == null) {
            Debug.Log("GSearchChase DetectionTransform was null, blacklisting");
            BlacklistAction();
            return false;
        }

        if (influenceMap == null) {
            Debug.Log("GSearchChase influenceMap was null, blacklisting");
            BlacklistAction();
            return false;
        }

        return true;
    }

    // Update is called once per frame
    private void Update() {
    }

    private void OnDrawGizmosSelected() {
        if (chasePosition != Vector3.zero) {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(chasePosition, 0.75f);
        }
    }
}
