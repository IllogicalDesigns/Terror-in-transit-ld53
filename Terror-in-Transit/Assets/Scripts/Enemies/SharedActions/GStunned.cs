using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GStunned : GAction {
    [SerializeField] private float stunTimer = 5f;

    public override void Interruppted() {
        gameObject.SendMessage("SetStun", false);
    }

    public override IEnumerator Perform() {
        gameObject.SendMessage("SetStun", true);
        gAgent.agent.isStopped = true;
        yield return new WaitForSeconds(stunTimer);
        gAgent.AddGoal("Search", 3, true);
        CompletedAction();
    }

    public override bool PostPerform() {
        gameObject.SendMessage("SetStun", false);
        return true;
    }

    public override bool PrePerform() {
        gAgent.agentState.SetState("AgressionLevel", GGhost.Agression.agressive);
        gameObject.SendMessage("DisableLightFor", stunTimer, SendMessageOptions.DontRequireReceiver);
        return true;
    }

    public override bool IsAchievable() {
        return base.IsAchievable();
    }

    // Start is called before the first frame update
    private void Start() {
        isInterruptible = false;

        AddEffects("Stunned");
    }

    // Update is called once per frame
    private void Update() {
    }
}
