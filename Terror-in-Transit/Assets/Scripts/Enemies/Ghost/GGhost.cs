using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GGhost : GAgent {
    [SerializeField] private AwarenessSystem awarenessSystem;

    [SerializeField] private TrackedTarget currentTarget;

    // Start is called before the first frame update
    public override void Start() {
        base.Start();

        AddGoal("Patrol", 5, false);
    }

    // Update is called once per frame
    private void FixedUpdate() {
    }
}
