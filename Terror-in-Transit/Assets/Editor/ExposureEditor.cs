using UnityEngine;
using System.Collections;
using UnityEditor;

//[CustomEditor(typeof(ExposureMapper))]
public class ExposureEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        //ExposureMapper exposureMap = (ExposureMapper)target;
        //if (GUILayout.Button("Build Exposure Map")) {
        //    exposureMap.ClearGraphEditor();
        //    exposureMap.BuildGraph();
        //}

        //if (GUILayout.Button("Clear Exposure Map")) {
        //    exposureMap.ClearGraphEditor();
        //}
    }
}
