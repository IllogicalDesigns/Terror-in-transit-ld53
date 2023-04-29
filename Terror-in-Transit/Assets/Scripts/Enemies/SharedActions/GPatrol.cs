using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GPatrol : GAction {

    [Header("Action specific settings")]
    [SerializeField] private PatrolPath patrolPath;

    [SerializeField] private float waitAtEachPoint = 1f;

    public override void Awake() {
        base.Awake();

        AddEffects("Patrol");
    }

    public override void Interruppted() {
    }

    public override IEnumerator Perform() {
        Vector3 localPosition = transform.position;
        var sortedPatrolPoints = patrolPath.points.OrderBy(obj => Vector3.Distance(obj.transform.position, localPosition)).ToList();

        var ClosestPatrolPoint = sortedPatrolPoints[0];
        var indexOfClosest = patrolPath.points.IndexOf(ClosestPatrolPoint);
        var numOfPatrols = patrolPath.points.Count;

        Queue<Transform> patrolQueue = new Queue<Transform>();

        for (int i = indexOfClosest; i < numOfPatrols + indexOfClosest; i++) {
            patrolQueue.Enqueue(patrolPath.points[i % numOfPatrols]);
        }

        while (patrolQueue.Count > 0) {
            Transform patrolPoint = patrolQueue.Dequeue();
            yield return AgentHelpers.GoToTransform(gAgent.agent, patrolPoint);
            yield return new WaitForSeconds(waitAtEachPoint);
        }

        CompletedAction();
    }

    public override bool PostPerform() {
        return true;
    }

    public override bool PrePerform() {
        return patrolPath != null;
    }

    public override bool IsAchievable() {
        if (patrolPath == null) return false;
        return base.IsAchievable();
    }

    // Update is called once per frame
    private void Update() {
    }
}
