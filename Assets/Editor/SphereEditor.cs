using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SphereGenerator))]
public class SphereEditor : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        SphereGenerator generator = (SphereGenerator)target;
        int edges = EditorGUILayout.IntSlider("Edges", generator.Edges, 3, 20);
        int layers = EditorGUILayout.IntSlider("Layers", generator.Layers, 2, 20);

        generator.Resize(edges, layers);
        generator.Generate();
    }

}
