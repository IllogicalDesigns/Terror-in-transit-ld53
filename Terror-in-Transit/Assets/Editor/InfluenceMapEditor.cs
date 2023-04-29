using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(InfluenceMap))]
public class InfluenceMapEditor : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        InfluenceMap myScript = (InfluenceMap)target;
        if (GUILayout.Button("Generate")) {
            myScript.GenerateGrid();
        }
    }
}
