using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vision : MonoBehaviour
{
    [SerializeField] CoffinDefinition coffin;
    [SerializeField] CoffinDefinition periCoffin;
    //[SerializeField] coneDefinition mainCone;
    //[SerializeField] coneDefinition secondCone;
    //[SerializeField] coneDefinition rearLeftCone;
    //[SerializeField] coneDefinition rearRightCone;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void DrawCoffin(Transform _transform, CoffinDefinition coffinSpecs) {
        //Start line
        Vector3 startLeft = _transform.forward * coffinSpecs.startDistance + -_transform.right * coffinSpecs.startWidth;
        Vector3 startRight = _transform.forward * coffinSpecs.startDistance + _transform.right * coffinSpecs.startWidth;

        //Mid line
        Vector3 midLeft = _transform.forward * coffinSpecs.midDistance + -_transform.right * coffinSpecs.midWidth;
        Vector3 midRight = _transform.forward * coffinSpecs.midDistance + _transform.right * coffinSpecs.midWidth;

        //end line
        Vector3 endLeft = _transform.forward * coffinSpecs.endDistance + -_transform.right * coffinSpecs.endWidth;
        Vector3 endRight = _transform.forward * coffinSpecs.endDistance + _transform.right * coffinSpecs.endWidth;

        startLeft.y = _transform.position.y + coffinSpecs.height;
        startRight.y = _transform.position.y + coffinSpecs.height;

        midLeft.y = _transform.position.y + coffinSpecs.height;
        midRight.y = _transform.position.y + coffinSpecs.height;

        endLeft.y = _transform.position.y + coffinSpecs.height;
        endRight.y = _transform.position.y + coffinSpecs.height;

        Gizmos.DrawLine(startLeft, startRight);

        Gizmos.DrawLine(startLeft, midLeft);
        Gizmos.DrawLine(startRight, midRight);

        Gizmos.DrawLine(midLeft, endLeft);
        Gizmos.DrawLine(midRight, endRight);

        Gizmos.DrawLine(endLeft, endRight);
    }

    void Draw3DCoffin(Transform _transform, CoffinDefinition coffinSpecs, float baseHeight = 0f) {
        DrawCoffin(transform, coffinSpecs);
        CoffinDefinition coffinFloor = new CoffinDefinition(coffinSpecs);
        coffinFloor.height = baseHeight;
        DrawCoffin(transform, coffinFloor);
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Draw3DCoffin(transform, coffin);

        Gizmos.color = Color.yellow;
        Draw3DCoffin(transform, periCoffin);
    }
}

//[System.Serializable]
//public class coneDefinition {
//    public Vector3 coneOffset = Vector3.zero;
//    public float anglesRange = 30;
//    public float radius = 10;
//    public float maxSteps = 20;
//    public float detectionMultipler = 1f;

//    public float maxWidth = 5f;
//}

[System.Serializable]
public class CoffinDefinition {
    public float startWidth = 5f;
    public float startDistance = -1f;

    public float midWidth = 10f;
    public float midDistance = 5f;

    public float endWidth = 5f;
    public float endDistance = 15f;

    public float height = 2f;

    public CoffinDefinition(CoffinDefinition coffinDef) {
        this.startWidth = coffinDef.startWidth;
        this.startDistance = coffinDef.startDistance;
        this.midWidth = coffinDef.midWidth;
        this.midDistance = coffinDef.midDistance;
        this.endWidth = coffinDef.endWidth;
        this.endDistance = coffinDef.endDistance;
        this.height = coffinDef.height;
    }

    public CoffinDefinition(float startWidth, float startDistance, float midWidth, float midDistance, float endWidth, float endDistance, float height) {
        this.startWidth = startWidth;
        this.startDistance = startDistance;
        this.midWidth = midWidth;
        this.midDistance = midDistance;
        this.endWidth = endWidth;
        this.endDistance = endDistance;
        this.height = height;
    }
}
