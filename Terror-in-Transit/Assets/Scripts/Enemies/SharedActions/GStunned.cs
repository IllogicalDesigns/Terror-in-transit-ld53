using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GStunned : GAction {
    [SerializeField] private float stunTimer = 5f;
    [SerializeField] private AudioSource snoreSrc;

    public override void Interruppted() {
        gameObject.SendMessage("SetStun", false);
        snoreSrc.Stop();
    }

    public override IEnumerator Perform() {
        snoreSrc.Play();
        gameObject.SendMessage("SetStun", true);
        gAgent.agent.isStopped = true;
        yield return new WaitForSeconds(stunTimer);
        gAgent.AddGoal("Search", 6, true);

        //Bark here to get all AIs to search
        gameObject.SendMessage("BarkLine", "Attacked");

        var guards = FindObjectsOfType<GGhost>();

        bool confirmed = false;
        foreach (var guard in guards) {
            guard.AddGoal("Search", 6, true);
            guard.Replan();

            if (confirmed || guard.gameObject == gameObject) continue;
            guard.SendMessage("BarkLine", "Confirm");
            confirmed = true;
        }

        CompletedAction();
    }

    public override bool PostPerform() {
        snoreSrc.Stop();
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
