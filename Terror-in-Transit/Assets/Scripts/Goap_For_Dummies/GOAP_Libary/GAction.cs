using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class GAction : MonoBehaviour {
    [HideInInspector] public string actionName = "dumbAction";
    [HideInInspector] public WorldState[] preConditions;
    [HideInInspector] public WorldState[] afterEffects;

    [HideInInspector] public Dictionary<string, object> preconditions;
    [HideInInspector] public Dictionary<string, object> effects;

    public WorldStates agentBeliefs;

    [Header("GAction settings")]
    public float cost = 1.0f;

    public bool running = false;

    public Coroutine performCoroutine = null;

    [HideInInspector] public GAgent gAgent;

    public bool isInterruptible = true;

    public float blackListTime = 1f;
    public bool isBlacklisted;

    public GAction() {
        preconditions = new Dictionary<string, object>();
        effects = new Dictionary<string, object>();
    }

    public virtual void Awake() {
        actionName = this.GetType().ToString();

        if (preConditions != null) {
            foreach (WorldState w in preConditions) {
                AddPreconditions(w.key, w.value);
            }
        }

        if (afterEffects != null) {
            foreach (WorldState w in afterEffects) {
                AddEffects(w.key, w.value);
            }
        }

        gAgent = gameObject.GetComponent<GAgent>();
    }

    public virtual float GetCost() {
        return cost;
    }

    public virtual bool IsAchievable() {
        if (isBlacklisted) return false;

        return true;
    }

    public virtual void BlacklistAction() {
        isBlacklisted = true;

        Invoke("UnBlacklistAction", blackListTime);
    }

    public virtual void UnBlacklistAction() {
        isBlacklisted = false;
    }

    public bool IsAchievableGiven(Dictionary<string, object> conditions) {
        foreach (KeyValuePair<string, object> p in preconditions) {
            if (!conditions.ContainsKey(p.Key))
                return false;
        }
        return true;
    }

    public Coroutine StartAction() {
        if (performCoroutine != null) StopCoroutine(performCoroutine);
        performCoroutine = StartCoroutine(Perform());
        running = true;

        return performCoroutine;
    }

    public virtual void CompletedAction() {
        gAgent.CompleteAction();
    }

    public void AddPreconditions(string key, object value = null) {
        preconditions.Add(key, value);
    }

    public void AddEffects(string key, object value = null) {
        effects.Add(key, value);
    }

    public abstract bool PrePerform();

    public abstract IEnumerator Perform();

    public abstract bool PostPerform();

    public abstract void Interruppted();
}
