using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class AgentPathImpossibleTest : MonoBehaviour {
    private NavMeshAgent agent;

    [SerializeField] private Transform target;

    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private float maxDist = 1f;

    // Start is called before the first frame update
    private void Start() {
        agent = gameObject.GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    private void Update() {
        agent.SetDestination(target.position);

        if (agent.pathStatus != NavMeshPathStatus.PathComplete)
            return;

        if (Vector3.Distance(agent.destination, target.position) < maxDist) {
            textMesh.text = "Can reach destination";
        }
        else {
            textMesh.text = "Cannot get to that destination destination";
        }

        //if (agent.pathStatus == NavMeshPathStatus.PathComplete) {
        //    textMesh.text = "PathComplete";
        //}
        //else if (agent.pathStatus == NavMeshPathStatus.PathInvalid) {
        //    textMesh.text = "PathInvalid";
        //}
        //else if (agent.pathStatus == NavMeshPathStatus.PathPartial) {
        //    textMesh.text = "PathPartial";
        //}
        //else {
        //    textMesh.text = "ITS FUCKED BOSS";
        //}
    }
}
