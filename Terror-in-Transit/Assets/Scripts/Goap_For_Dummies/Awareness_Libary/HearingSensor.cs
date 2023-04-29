using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

public class HearingSensor : MonoBehaviour {
    [SerializeField] private AwarenessSystem awareness;

    [SerializeField] private float hearingRange = 30f;
    [SerializeField] private float ofScreenHearingRange = 10f;
    [SerializeField] private Color hearingColor = new Color(1f, 1f, 0f, 0.25f);

    [SerializeField] private LayerMask playerSightTestMask;
    [SerializeField] private Transform player;

    private bool onScreen;

    private bool isDebug = false;

    // Start is called before the first frame update
    private void Start() {
        HearingManager.Instance.Register(this);
        awareness = gameObject.GetComponent<AwarenessSystem>();
    }

    private void OnEnable() {
        HearingManager.Instance.Register(this);
    }

    private void OnDisable() {
        HearingManager.Instance.Register(this);
    }

    private void OnDestroy() {
        if (HearingManager.Instance) HearingManager.Instance.DeRegister(this);
    }

    // Update is called once per frame
    private void Update() {
    }

    private void OnDrawGizmosSelected() {
        var m_Renderer = GetComponent<Renderer>();

        Gizmos.color = hearingColor;
        Gizmos.DrawWireSphere(transform.position, onScreen ? hearingRange : ofScreenHearingRange);
    }

    public void OnHeardSound(GameObject source, HearingManager.EHeardSoundCategory heardSoundCategory, float intensity, float overrideHearingDist = 0) {
        onScreen = OnScreen();

        if (overrideHearingDist == 0) {
            overrideHearingDist = (onScreen ? hearingRange : ofScreenHearingRange);
        }

        var distToSound = Vector3.Distance(source.transform.position, transform.position);

        if (distToSound > overrideHearingDist) return;

        float totalDist = DistanceOnNavMesh();

        if (isDebug) Debug.Log("distToSound:" + distToSound + " totalDist:" + totalDist);

        if (totalDist > overrideHearingDist) return;

        awareness.ReportCanHear(source, heardSoundCategory, intensity);

        float DistanceOnNavMesh() {
            float totalDist = 0f;
            NavMeshPath path = new NavMeshPath();
            NavMesh.CalculatePath(transform.position, source.transform.position, NavMesh.AllAreas, path);

            for (int i = 0; i < path.corners.Length - 1; i++) {
                totalDist += Vector3.Distance(path.corners[i], path.corners[i + 1]);
                Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.cyan, 5f);
            }

            return totalDist;
        }
    }

    private bool OnScreen() {
        Vector3 eyeHeight = Vector3.up;
        Vector3 directionToObject = player.position - transform.position;

        float dotProduct = Vector3.Dot(player.forward, directionToObject.normalized);

        if (dotProduct < 0f && !Physics.Linecast(transform.position + eyeHeight, player.position + eyeHeight, playerSightTestMask)) return true;

        return false;
    }
}
