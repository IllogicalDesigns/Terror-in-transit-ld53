using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GPatrol : GAction {

    [Header("Action specific settings")]
    [SerializeField] private PatrolPath patrolPath;

    [SerializeField] private float waitAtEachPoint = 1f;
    [SerializeField] private float turnSpeed = 60;
    [SerializeField] private float agentSpeed = 1f;

    [SerializeField] private float closeDist = 2f;

    private Coroutine coroutine;

    public override void Awake() {
        base.Awake();

        AddEffects("Patrol");
    }

    public override void Interruppted() {
        StopAllCoroutines();
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

            if (gAgent.isBehind(patrolPoint.position)) {
                float angleThreshold = 1f;
                float rotationTime = 2f;

                gAgent.agent.isStopped = true;

                Quaternion targetRotation = Quaternion.LookRotation(patrolPoint.position - transform.position, Vector3.up);
                Quaternion startRotation = transform.rotation;
                float elapsedTime = 0f;

                while (elapsedTime < rotationTime && Quaternion.Angle(transform.rotation, targetRotation) > angleThreshold) {
                    Quaternion newRotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / rotationTime);
                    transform.rotation = newRotation;
                    elapsedTime += Time.deltaTime;
                    yield return new WaitForFixedUpdate(); ;
                }
            }

            do {
                gAgent.agent.isStopped = false;
                gAgent.agent.SetDestination(patrolPoint.position);
                yield return new WaitForSeconds(0.1f);
            } while (Vector3.Distance(transform.position, patrolPoint.position) > closeDist);

            //yield return AgentHelpers.GoToTransform(gAgent.agent, patrolPoint);
            yield return new WaitForSeconds(waitAtEachPoint);
        }

        CompletedAction();
    }

    public override bool PostPerform() {
        return true;
    }

    public override bool PrePerform() {
        gameObject.SendMessage("SetLightColor", LightColor.LightAwareness.unaware, SendMessageOptions.DontRequireReceiver);
        gAgent.agent.speed = agentSpeed;
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
