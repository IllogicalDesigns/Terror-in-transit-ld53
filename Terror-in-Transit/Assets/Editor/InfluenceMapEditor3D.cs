using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(InfluenceMap3D))]
public class InfluenceMapEditor3D : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        InfluenceMap3D influenceMapSCript = (InfluenceMap3D)target;
        if (GUILayout.Button("Generate")) {
            influenceMapSCript.BuildGrid();
        }

        if (GUILayout.Button("Clear")) {
            influenceMapSCript.ClearGrid();
        }
    }
}
