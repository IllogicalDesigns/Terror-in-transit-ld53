using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GNode {
    public GNode parent;
    public float cost;
    public Dictionary<string, object> state;
    public GAction action;

    public GNode(GNode parent, float cost, Dictionary<string, object> allStates, GAction action) {
        this.parent = parent;
        this.cost = cost;
        this.state = new Dictionary<string, object>(allStates); //copy of allStates
        this.action = action;
    }
}

public class GPlanner {

    public Queue<GAction> Plan(List<GAction> actions, Dictionary<string, object> goal, WorldStates agentProvidedStates) {  //agentProvidedStates, last known as states
        var combinedStates = GWorld.Instance.GetWorld().GetStates();
        if (agentProvidedStates != null) {
            combinedStates = combinedStates.Union(agentProvidedStates.states).ToDictionary(k => k.Key, v => v.Value);
        }

        List<GAction> useableActions = new List<GAction>();
        foreach (var a in actions) {
            if (a.IsAchievable())
                useableActions.Add(a);
        }

        List<GNode> leaves = new List<GNode>();
        GNode start = new GNode(null, 0, combinedStates, null);

        bool success = BuildGraph(start, leaves, useableActions, goal);

        if (goal.Count == 0) {
            Debug.Log("No Goals, so No plan was found");
            return null;
        }

        if (!success) {
            Debug.LogWarning("No plan was found");
            return null;
        }

        //Find cheapest leaf node
        GNode cheapest = null;
        foreach (var leaf in leaves) {
            if (cheapest == null)
                cheapest = leaf;
            else {
                if (leaf.cost < cheapest.cost)
                    cheapest = leaf;
            }
        }

        //Work backwards through the cheapest and create a linked list of nodes
        List<GAction> result = new List<GAction>();
        GNode n = cheapest;
        while (n != null) {
            if (n.action != null) {
                result.Insert(0, n.action);
            }
            n = n.parent;
        }

        Queue<GAction> queue = new Queue<GAction>();
        foreach (var a in result) {
            queue.Enqueue(a);
        }

        string planDebug = "";
        foreach (var a in queue) {
            planDebug = planDebug + " | " + a.actionName.ToString() + "\\n";
        }

        Debug.Log(" The plan is: " + planDebug);

        return queue;
    }

    private bool BuildGraph(GNode parent, List<GNode> leaves, List<GAction> useableActions, Dictionary<string, object> goal) {
        bool foundPath = false;
        foreach (var action in useableActions) {
            if (action.IsAchievableGiven(parent.state)) {
                Dictionary<string, object> currentState = new Dictionary<string, object>(parent.state);
                foreach (var eff in action.effects) {
                    if (!currentState.ContainsKey(eff.Key))
                        currentState.Add(eff.Key, eff.Value);
                }

                GNode node = new GNode(parent, parent.cost + action.GetCost(), currentState, action);

                if (GoalAchieved(goal, currentState)) {
                    leaves.Add(node);
                    foundPath = true;
                }
                else {
                    //check a new subset of nodes, minus the current action node
                    List<GAction> subset = ActionSubset(useableActions, action);
                    bool found = BuildGraph(node, leaves, subset, goal);
                    if (found)
                        foundPath = true;
                }
            }
        }
        return foundPath;
    }

    private bool GoalAchieved(Dictionary<string, object> goal, Dictionary<string, object> state) {
        foreach (var g in goal) {
            if (!state.ContainsKey(g.Key))
                return false;
        }
        return true;
    }

    //TODO cdg can this be optimized, does it need to be optimized
    private List<GAction> ActionSubset(List<GAction> actions, GAction removeMe) {
        List<GAction> subset = new List<GAction>();
        foreach (var a in actions) {
            if (!a.Equals(removeMe))
                subset.Add(a);
        }
        return subset;
    }
}
