using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif // UNITY_EDITOR

public class VisionSensor : MonoBehaviour {
    [SerializeField] private LayerMask detectionMask = ~0; // default to everything

    [SerializeField] private float coneRange = 5f, visionRange = 30f, visionAngle = 60f;
    [SerializeField] private float coneCloseRange = 5f, coneCloseAngle = 80;

    private Color visionColor = new Color(1f, 0f, 0f, 1f);
    private Color rangeColor = new Color(1f, 1f, 1f, 0.5f);

    public float cosVisConeAngle { get; private set; } = 0f;

    [SerializeField] public Vector3 eyeLocation => transform.position + Vector3.up;
    public Vector3 eyeDirection => transform.forward;

    [SerializeField] private AwarenessSystem awareness;

    [SerializeField] private bool isDebug = true;

    [SerializeField] private CoffinDefinition primaryCoffin, secondaryCoffin;
    [SerializeField] private ShoulderZoneDefinition shoulderZone;

    private float peripheryMulti = 0.5f, primaryMulti = 1f;

    private void Awake() {
        cosVisConeAngle = Mathf.Cos(visionAngle * Mathf.Deg2Rad);
    }

    // Start is called before the first frame update
    private void Start() {
        awareness = gameObject.GetComponent<AwarenessSystem>();
    }

    public float CanWeSeeTarget(GameObject candidateGameObject) {
        var candidatePosition = candidateGameObject.transform.position;

        if (!isValid()) return 0f;

        if (!isInRange()) return 0f;

        Vector3 direction = candidatePosition - transform.position;

        bool lineOfSight = hasLineOfSight();
        if (!lineOfSight) return 0f;

        bool inSecondaryVisionCoffin = inSecondaryCoffin();
        if (!inSecondaryVisionCoffin) return 0f;

        bool inPrimaryVisionCoffin = inPrimaryCoffin();
        bool inRearVisionZone = inOverShoulderZone();

        if (!inPrimaryVisionCoffin && !inRearVisionZone) return peripheryMulti;

        return primaryMulti;

        bool isValid() {
            //Don't detect ourseleves
            if (candidateGameObject == gameObject) return false;

            if (isDebug) Debug.DrawLine(transform.position, candidatePosition, Color.gray);

            return true;
        }

        bool isInRange() {
            if (Vector3.Distance(candidatePosition, transform.position) < visionRange) {
                if (isDebug) Debug.DrawLine(transform.position, candidatePosition, Color.white);
                return true;
            }

            return false;
        }

        bool isInCone() {
            float angle = Vector3.Angle(direction, transform.forward);

            if (angle <= visionAngle * 0.5f && direction.magnitude <= coneRange) {
                if (isDebug) Debug.DrawLine(transform.position, candidatePosition, Color.yellow);
                return true;
            }

            return false;
        }

        bool hasLineOfSight() {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction.normalized, out hit, visionRange, detectionMask)) {
                if (hit.collider.gameObject == candidateGameObject) {
                    if (isDebug) Debug.DrawLine(transform.position, candidatePosition, Color.green);
                    return true;
                }
                else if (isDebug) Debug.DrawLine(transform.position, candidatePosition, Color.yellow);
            }

            return false;
        }

        bool inPrimaryCoffin() {
            return CheckCoffin(primaryCoffin.farWidth, primaryCoffin.farDist, primaryCoffin.midWidth, primaryCoffin.midDist, primaryCoffin.closeWidth, primaryCoffin.closeDist, candidatePosition);
        }

        bool inSecondaryCoffin() {
            secondaryCoffin.farDist = visionRange;

            return CheckCoffin(secondaryCoffin.farWidth, secondaryCoffin.farDist, secondaryCoffin.midWidth, secondaryCoffin.midDist, secondaryCoffin.closeWidth, secondaryCoffin.closeDist, candidatePosition);
        }

        bool inOverShoulderZone() {
            if (Vector3.Distance(candidatePosition, transform.position) > Mathf.Abs(shoulderZone.farDist) + shoulderZone.farWidth) return false;

            bool shoulder = CheckShoulderShape(shoulderZone.closeWidth, shoulderZone.closeDist, shoulderZone.farWidth, shoulderZone.farDist, candidatePosition);
            if (!shoulder) shoulder = CheckShoulderShape(-shoulderZone.closeWidth, shoulderZone.closeDist, -shoulderZone.farWidth, shoulderZone.farDist, candidatePosition);

            return shoulder;
        }
    }

    private bool CheckCoffin(float farWidth, float farDist, float midWidth, float midDist, float closeWidth, float closeDist, Vector3 candidatePosition) {
        Vector3 farLeft = (transform.position + transform.forward * farDist) + transform.right * -farWidth;
        Vector3 farRight = transform.position + transform.forward * farDist + transform.right * farWidth;

        Vector3 midLeft = (transform.position + transform.forward * midDist) + transform.right * -midWidth;
        Vector3 midRight = transform.position + transform.forward * midDist + transform.right * midWidth;

        Vector3 closeLeft = (transform.position + transform.forward * closeDist) + transform.right * -closeWidth;
        Vector3 closeRight = transform.position + transform.forward * closeDist + transform.right * closeWidth;

        bool isInCoffin = false;
        if (CheckTrapezoid(closeLeft, closeRight, midLeft, midRight, candidatePosition))
            isInCoffin = true;

        if (CheckTrapezoid(midLeft, midRight, farLeft, farRight, candidatePosition))
            isInCoffin = true;

        return isInCoffin;
    }

    private bool CheckTrapezoid(Vector3 closeLeft, Vector3 closeRight, Vector3 midLeft, Vector3 midRight, Vector3 candidatePosition) {
        bool isInTrapezoid = false;
        if (CheckTriangle(closeLeft, closeRight, midLeft, candidatePosition))
            isInTrapezoid = true;

        if (CheckTriangle(midLeft, midRight, closeRight, candidatePosition))
            isInTrapezoid = true;

        return isInTrapezoid;
    }

    private bool CheckShoulderShape(float closeWidth, float closeDist, float farWidth, float farDist, Vector3 candidatePosition) {
        var closeVec1 = (transform.position + transform.forward * closeDist) + transform.right * -closeWidth;
        var closeVec2 = (transform.position + transform.forward * closeDist) + transform.right * (-closeWidth * 0.5f);

        var farVec1 = (transform.position + transform.forward * farDist) + transform.right * -farWidth;
        var farVec2 = (transform.position + transform.forward * farDist) + transform.right * (-farWidth * 0.5f);

        bool pointInSpace = CheckTrapezoid(closeVec1, closeVec2, farVec1, farVec2, candidatePosition);
        if (!pointInSpace) pointInSpace = CheckTriangle(closeVec2, farVec2, transform.position, candidatePosition);

        return pointInSpace;
    }

    private bool CheckTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 p) {
        // Compute vectors
        var v0 = c - a;
        var v1 = b - a;
        var v2 = p - a;

        // Compute dot products
        var dot00 = Vector3.Dot(v0, v0);
        var dot01 = Vector3.Dot(v0, v1);
        var dot02 = Vector3.Dot(v0, v2);
        var dot11 = Vector3.Dot(v1, v1);
        var dot12 = Vector3.Dot(v1, v2);

        // Compute barycentric coordinates
        var invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
        var u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        var v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        // Check if point is in triangle
        bool isInTri = (u >= 0) && (v >= 0) && (u + v < 1);

        if (isDebug) {
            Color clr = !isInTri ? Color.red : Color.green;
            Debug.DrawLine(a, b, clr);
            Debug.DrawLine(b, c, clr);
            Debug.DrawLine(c, a, clr);
        }

        return isInTri;
    }

    // Update is called once per frame
    private void Update() {
        for (int i = 0; i < DeteactableTargetManager.Instance.AllTargets.Count; i++) {
            var candidateTarget = DeteactableTargetManager.Instance.AllTargets[i];

            var visionMulti = CanWeSeeTarget(candidateTarget.gameObject);
            if (visionMulti != 0f)
                awareness.ReportCanSee(candidateTarget, visionMulti);
        }
    }

    [System.Serializable]
    public class CoffinDefinition {
        public float farWidth = 2.5f;
        public float farDist = 15f;

        public float midWidth = 4f;
        public float midDist = 5f;

        public float closeWidth = 1f;
        public float closeDist = 0f;
    }

    [System.Serializable]
    public class ShoulderZoneDefinition {
        public float closeWidth = 2f;
        public float closeDist = 0f;

        public float farWidth = 2f;
        public float farDist = -1;
    }

    //#if UNITY_EDITOR
    //private void OnDrawGizmos() {
    //    Gizmos.color = visionColor;
    //    //GizmosExtensions.DrawWireArc(transform.position, transform.forward, visionAngle, coneRange, 10);
    //    Gizmos.color = rangeColor;
    //    Gizmos.DrawWireSphere(transform.position, visionRange);

    //    Gizmos.color = Color.magenta;

    //    for (int i = 0; i < DeteactableTargetManager.Instance.AllTargets.Count; i++) {
    //        var candidateTarget = DeteactableTargetManager.Instance.AllTargets[i];

    //        Gizmos.DrawWireSphere(candidateTarget.transform.position, 0.5f);
    //    }
    //}

    //#endif // UNITY_EDITOR
}
