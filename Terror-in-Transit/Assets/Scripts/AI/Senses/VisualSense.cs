using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualSense : MonoBehaviour
{
    [SerializeField] coneDefinition mainCone;
    [SerializeField] coneDefinition secondCone;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class coneDefinition {
    public Vector3 coneOffset = Vector3.zero;
    public float anglesRange = 30;
    public float radius = 10;
    public float maxSteps = 20;
    public float detectionMultipler = 1f;

    public float maxWidth = 5f;
}
