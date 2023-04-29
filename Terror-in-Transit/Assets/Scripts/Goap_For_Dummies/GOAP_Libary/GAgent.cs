using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

public class SubGoal {
    public Dictionary<string, object> sgoals;
    public bool isRemoveable;

    public SubGoal(string str, int priority, bool _removable) {
        sgoals = new Dictionary<string, object>();
        sgoals.Add(str, priority);
        isRemoveable = _removable;
    }
}

public class GAgent : MonoBehaviour {
    public List<GAction> actions = new List<GAction>();
    [SerializeField] public Dictionary<SubGoal, int> goals = new Dictionary<SubGoal, int>();

    private GPlanner planner;
    public Queue<GAction> actionQueue;
    public GAction currentAction;
    public SubGoal currentGoal;

    private Coroutine currentActionRoutine;

    public WorldStates agentState = new WorldStates();

    public NavMeshAgent agent;

    private Collider[] Colliders = new Collider[150]; // more is less performant, but more options

    // Start is called before the first frame update
    private void Awake() {
        agent = gameObject.GetComponent<NavMeshAgent>();
    }

    public virtual void Start() {
        GAction[] acts = this.GetComponents<GAction>();
        foreach (var a in acts) {
            actions.Add(a);
        }
    }

    public void CompleteAction() {
        if (currentAction) {
            StopCoroutine(currentAction.performCoroutine);
            currentAction.performCoroutine = null;
            currentAction.running = false;
            currentAction.PostPerform();
        }
    }

    public void Replan(bool force = false) {
        if (currentAction == null) {
            //Debug.Log("Current action is null");
            return;
        }

        if (!force && !currentAction.isInterruptible) {
            Debug.Log(currentAction.name + " is not interuptable, preventing interupt during replan");
            return;
        }

        actionQueue = null;

        currentAction.Interruppted();
        StopCoroutine(currentActionRoutine);
        currentAction.running = false;
        currentAction = null;
    }

    public bool AddGoal(string str, int priority, bool _removable) {
        if (HasGoal(str)) return false;

        SubGoal newSub = new SubGoal(str, priority, _removable);
        goals.Add(newSub, priority);

        return true;
    }

    public bool HasGoal(string str) {
        foreach (var i in goals.Keys) {
            if (i.sgoals.ContainsKey(str))
                return true;
        }

        return false;
    }

    public void RemoveGoal(string str) {
        foreach (var i in goals.Keys) {
            if (i.sgoals.ContainsKey(str))
                i.sgoals.Remove(str);
        }

        return;
    }

    // Update is called once per frame
    private void LateUpdate() {
        //We have a current running action, let it run
        if (currentAction != null && currentActionRoutine != null && currentAction.running) return;

        //agent has no plan
        if (planner == null || actionQueue == null) {
            planner = new GPlanner();

            //sory by decending priority our goals using Linq
            var sortedGoals = from entry in goals orderby entry.Value ascending select entry;

            foreach (var sg in sortedGoals) {
                actionQueue = planner.Plan(actions, sg.Key.sgoals, agentState); //todo cdg is the null where we pass in our local state?
                if (actionQueue != null) {
                    currentGoal = sg.Key;
                    break;
                }
            }
        }

        //if we run out of things todo, remove the goal and setup to get new plan
        if (actionQueue != null && actionQueue.Count == 0) {
            if (currentGoal.isRemoveable) {
                goals.Remove(currentGoal);
            }
            planner = null;
        }

        //We have a queue and no action running, start the next action.
        if (actionQueue != null && actionQueue.Count > 0) {
            currentAction = actionQueue.Dequeue();

            //Attempt to run the preperform, if fails, start a new plan
            if (currentAction.PrePerform()) {
                currentActionRoutine = currentAction.StartAction();
            }
            else {
                actionQueue = null; //force a replan as an action failed
            }
        }
    }

    public virtual void OnSuspicious(TrackedTarget target) {
    }

    public virtual void OnLostSuspicion(TrackedTarget target) {
    }

    public virtual void OnDetected(TrackedTarget target) {
    }

    public virtual void OnFullyDetected(TrackedTarget target) {
    }

    public virtual void OnLostDetection(TrackedTarget target) {
    }

    public virtual void OnFullyLost(TrackedTarget target) {
    }
}
