using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class AgentHelpers {

    public static int ColliderArraySortComparer(Collider A, Collider B, Transform transform) {
        if (A == null && B != null) {
            return 1;
        }
        else if (A != null && B == null) {
            return -1;
        }
        else if (A == null && B == null) {
            return 0;
        }
        else {
            return Vector3.Distance(transform.position, A.transform.position).CompareTo(Vector3.Distance(transform.position, B.transform.position));
        }
    }

    public static IEnumerator RotateToFaceTarget(Transform transform, Transform target, float turnSpeed = 360f) {
        float timeOut = 5f;
        do {
            var targetDirection = target.position - transform.position;
            targetDirection.y = 0.00F; // Lock global y-axis

            var targetRotation = Quaternion.LookRotation(targetDirection);
            var deltaAngle = Quaternion.Angle(transform.rotation, targetRotation);

            if (deltaAngle == 0.00F) { // Exit early if no update required
                yield break;
            }

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime / deltaAngle
            );

            timeOut -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        } while (timeOut > 0f);
    }

    public static IEnumerator RotateToFaceTarget(Transform transform, Vector3 target, float turnSpeed = 360f) {
        float timeOut = 5f;
        do {
            var targetDirection = target - transform.position;
            targetDirection.y = 0.00F; // Lock global y-axis

            var targetRotation = Quaternion.LookRotation(targetDirection);
            var deltaAngle = Quaternion.Angle(transform.rotation, targetRotation);

            if (deltaAngle == 0.00F) { // Exit early if no update required
                yield break;
            }

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime / deltaAngle
            );

            timeOut -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        } while (timeOut > 0f);
    }

    public static Vector3 CheckAndNudgePosAll(Vector3 position, float check = 1f, float nudge = 1f) {
        position = CheckAndNudgePos(position, Vector3.left, check, nudge);
        position = CheckAndNudgePos(position, Vector3.right, check, nudge);
        position = CheckAndNudgePos(position, Vector3.forward, check, nudge);
        position = CheckAndNudgePos(position, Vector3.back, check, nudge);
        return position;
    }

    public static Vector3 CheckAndNudgePos(Vector3 position, Vector3 dir, float check = 1f, float nudge = 1f) {
        var testPos = position + (dir * check);

        NavMeshHit hit;
        bool blocked = NavMesh.Raycast(position, testPos, out hit, NavMesh.AllAreas);
        Debug.DrawLine(position, testPos, blocked ? Color.red : Color.green, 10f);

        if (blocked) {
            position += dir * -nudge;
            Debug.DrawRay(hit.position, Vector3.up, Color.red, 10f);
        }

        return position;
    }

    public static IEnumerator GoToTransform(NavMeshAgent agent, Transform target, float stopDist = 2f, float timeOut = 15f) {
        agent.stoppingDistance = stopDist - 1;
        agent.isStopped = false;

        do {
            agent.SetDestination(target.position);

            yield return new WaitForFixedUpdate();
            timeOut -= Time.fixedDeltaTime;

            //Debug.Log(DistanceOnNavMesh(agent.path, agent.transform.position));
            //} while (timeOut > 0f && DistanceOnNavMesh(agent.path, agent.transform.position) > stopDist);

            //do {
            //    //agent.SetDestination(target.position);

            //    do {
            //        yield return new WaitForFixedUpdate();
            //    } while (agent.path.status == NavMeshPathStatus.PathInvalid);

            //    timeOut -= Time.deltaTime;
        } while (timeOut > 0f && Vector3.Distance(target.position, agent.transform.position) > stopDist); //DistanceOfPath(agent.path, agent) > stopDist

        agent.isStopped = true;
    }

    private static float DistanceOnNavMesh(NavMeshPath path, Vector3 currentPos) {
        if (path.status != NavMeshPathStatus.PathComplete) return Mathf.Infinity;

        float totalDist = 0f;

        for (int i = 0; i < path.corners.Length - 1; i++) {
            var corner = (i == 0) ? currentPos : path.corners[i];
            totalDist += Vector3.Distance(corner, path.corners[i + 1]);
            Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.cyan, 1f);
        }

        return totalDist;
    }

    public static IEnumerator GoToPosition(NavMeshAgent agent, Vector3 target, float stopDist = 2f, float timeOut = 15f) {
        agent.stoppingDistance = stopDist - 1;
        agent.isStopped = false;

        do {
            agent.SetDestination(target);

            do {
                yield return new WaitForFixedUpdate();
            } while (agent.path.status == NavMeshPathStatus.PathInvalid);

            timeOut -= Time.deltaTime;
        } while (timeOut > 0f && Vector3.Distance(agent.pathEndPosition, agent.transform.position) > stopDist * 2f); //DistanceOfPath(agent.path, agent) > stopDist

        if (timeOut <= 0) Debug.Log("GoToPosition had to force stop based on timeout!!!! this is bad!");
    }

    public static float distOnNavMesh(Vector3 start, Vector3 end, int areaMask = NavMesh.AllAreas) {
        var Path = new NavMeshPath();
        float distance = 0f;

        if (NavMesh.CalculatePath(start, end, areaMask, Path)) {
            distance = Vector3.Distance(start, Path.corners[0]);

            for (int j = 1; j < Path.corners.Length; j++) {
                Debug.DrawLine(Path.corners[j - 1], Path.corners[j], Color.red, 2f);
                distance += Vector3.Distance(Path.corners[j - 1], Path.corners[j]);
            }
        }

        return distance;
    }

    public static float visibleDistOnNavMesh(Vector3 start, Vector3 end, Transform player, NavMeshAgent agent, float visibilityPenalty = 1.5f, float dirOfPlayerPenalty = 2f, float pointIsVisblePenalty = 100f) {
        var Path = new NavMeshPath();
        float distance = 0f;

        if (NavMesh.CalculatePath(start, end, agent.areaMask, Path)) {
            distance = Vector3.Distance(start, Path.corners[0]);

            //End point is visible, add a penalty to its value
            if (!Physics.Linecast(end, player.position + Vector3.up)) {
                distance += pointIsVisblePenalty;
            }

            //Loop through all coners, check if its visible, add its distance and penalty values
            for (int j = 1; j < Path.corners.Length; j++) {
                var dir = GetDirectionBetweenPoints(Path.corners[j - 1], Path.corners[j]);
                var dot = Vector3.Dot(dir, player.forward);

                float visMod = 1f;
                if (!Physics.Linecast(Path.corners[j], player.position + Vector3.up)) {
                    visMod = visibilityPenalty;

                    if (dot < 0.5f) {
                        Debug.DrawLine(Path.corners[j - 1], Path.corners[j], Color.yellow, 2f);
                    }
                    else {
                        visMod += dirOfPlayerPenalty;
                        Debug.DrawLine(Path.corners[j - 1], Path.corners[j], Color.green, 2f);
                    }
                }
                else {
                    Debug.DrawLine(Path.corners[j - 1], Path.corners[j], Color.red, 2f);
                }

                distance += Vector3.Distance(Path.corners[j - 1], Path.corners[j]) * visMod;
            }
        }

        return distance;
    }

    public static Vector3 GetDirectionBetweenPoints(Vector3 start, Vector3 target) {
        var heading = start - target;

        var distance = heading.magnitude;
        var direction = heading / distance; // This is now the normalized direction.

        return direction;
    }

    public static bool HasAgentReachedDestination(NavMeshAgent agent) {
        // Check if we've reached the destination
        if (!agent.pathPending) return false;
        if (agent.remainingDistance <= agent.stoppingDistance) return false;
        if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f) return true;

        return false;
    }

    public static Vector3 SamplePointOnNavMesh(Vector3 point, float repositionDist = 2f, int areaMask = NavMesh.AllAreas) {
        Vector3 result = Vector3.zero;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(point, out hit, repositionDist, areaMask)) {
            result = hit.position;
        }

        return result;
    }

    public static float DistanceOfPath(NavMeshPath path, NavMeshAgent agent) {
        var overallDistance = 0f;
        for (int i = 0; i < path.corners.Length - 1; i++) {
            Vector3 vec = path.corners[i + 1];

            if (path.corners.Length - 1 == i + 1)
                vec = agent.pathEndPosition;

            overallDistance += Vector3.Distance(path.corners[i], vec);
        }
        //Debug.Log("path status:" + path.status + " dist: " + Vector3.Distance(path.corners[0], path.corners[path.corners.Length - 1]) + " vs nav: " + overallDistance);

        return overallDistance;
    }
}
