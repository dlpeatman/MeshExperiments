using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Cubes))]
public class CubesEditor : Editor {
    public override void OnInspectorGUI() {

        Cubes cubes = (Cubes)target;
        int width = EditorGUILayout.IntSlider("Width", cubes.Width, 1, 20);
        int length = EditorGUILayout.IntSlider("Length", cubes.Length, 1, 20);

        cubes.Reset(width, length);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate")) {
            Debug.Log("Manually Generating...");
            cubes.Generate();
        }
        if (GUILayout.Button("Clear")) {
            Debug.Log("Clearing...");
            cubes.CleanUp();
        }
        GUILayout.EndHorizontal();
    }
}
