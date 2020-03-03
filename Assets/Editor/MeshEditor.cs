using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CustomEditor(typeof(MeshGenerator))]
public class MeshEditor : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        MeshGenerator generator = (MeshGenerator)target;

        int xSize = EditorGUILayout.IntSlider("X Quads", generator.XQuads, 1, 100);
        int zSize = EditorGUILayout.IntSlider("X Quads", generator.ZQuads, 1, 100);
        float xScale = EditorGUILayout.Slider("X Scale", generator.XScale, .1f, 5);
        float zScale = EditorGUILayout.Slider("Z Scale", generator.ZScale, .1f, 5);
        float xFactor = EditorGUILayout.Slider("X Factor", generator.XFactor, .1f, 2);
        float zFactor = EditorGUILayout.Slider("Z Factor", generator.ZFactor, .1f, 2);
        float xyFactor = EditorGUILayout.Slider("X-Y Factor", generator.XyFactor, -10, 10);
        float zyFactor = EditorGUILayout.Slider("Z-Y Factor", generator.ZyFactor, -10, 10);
        generator.Resize(xSize, zSize, xScale, zScale, xFactor, zFactor, xyFactor, zyFactor);
        generator.Generate();
    }

}
