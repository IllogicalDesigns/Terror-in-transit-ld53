using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(GAgentVisual))]
[CanEditMultipleObjects]
public class GAgentVisualEditor : Editor {

    private void OnEnable() {
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        serializedObject.Update();
        GAgentVisual agent = (GAgentVisual)target;
        GUILayout.Label("Name: " + agent.name);
        GUILayout.Label("Current Action: " + agent.gameObject.GetComponent<GAgent>().currentAction);
        GUILayout.Label("Current Goal: " + agent.gameObject.GetComponent<GAgent>().currentGoal);
        GUILayout.Label("Actions: ");

        foreach (GAction a in agent.gameObject.GetComponent<GAgent>().actions) {
            string pre = "";
            string eff = "";
            string c = a.GetCost().ToString();

            foreach (KeyValuePair<string, object> p in a.preconditions)
                pre += p.Key + ", ";
            foreach (KeyValuePair<string, object> e in a.effects)
                eff += e.Key + ", ";

            GUILayout.Label("====  " + a.actionName + "(p:" + pre + ")(e:" + eff + ")" + "($:" + c + ")");
        }

        GUILayout.Label("Goals: ");
        foreach (KeyValuePair<SubGoal, int> g in from entry in agent.gameObject.GetComponent<GAgent>().goals orderby entry.Value ascending select entry) {
            var goals = g.Key.sgoals;
            foreach (var sg in goals) {
                GUILayout.Label("==[" + g.Value + "]: " + sg.Key);
            }
        }

        if (agent.gameObject.GetComponent<GAgent>().agentState == null) return;

        GUILayout.Label("===== AgentState: ");
        foreach (KeyValuePair<string, object> p in agent.gameObject.GetComponent<GAgent>()?.agentState?.GetStates()) {
            if (p.Value is TrackedTarget) {
                var tracked = (TrackedTarget)p.Value;
                if (tracked != null && tracked.stimulator != null)
                    GUILayout.Label(p.Key + " " + tracked.stimulator.name);
            }
            else {
                var val = (p.Value == null) ? "Null" : p.Value.ToString();
                GUILayout.Label(p.Key + " " + val);
            }
        }

        if (agent.gameObject.GetComponent<GAgent>().currentAction == null) return;
        if (agent.gameObject.GetComponent<GAgent>().actionQueue == null) return;

        GUILayout.Label("===== CurrentPlan: ");
        GUILayout.Label(agent?.gameObject.GetComponent<GAgent>()?.currentAction?.ToString());
        foreach (var p in agent?.gameObject.GetComponent<GAgent>()?.actionQueue) {
            GUILayout.Label(p.ToString());
        }

        //if (agent?.gameObject.GetComponent<AwarenessSystem>().GetTargets().Count > 0) return;

        //GUILayout.Label("===== Awareness: ");
        //foreach (var p in agent?.gameObject.GetComponent<AwarenessSystem>().GetTargets()) {
        //    GUILayout.Label(p.stimulator.name.ToString() + " " + p.rawPosition.ToString() + " " + p.detectable == null? "Visual/Prox [" : "Sound [" + p.awareness.ToString() + "]");
        //}
        //serializedObject.ApplyModifiedProperties();
    }
}
