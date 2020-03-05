using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Cubes))]
public class CubesEditor : Editor {
    public override void OnInspectorGUI() {

        Cubes cubes = (Cubes)target;
        cubes.width = EditorGUILayout.IntSlider("Width", cubes.width, 1, 16);
        cubes.length = EditorGUILayout.IntSlider("Length", cubes.length, 1, 16);
        cubes.height = EditorGUILayout.IntSlider("Height", cubes.height, 1, 16);
        cubes.xNoiseOffset = EditorGUILayout.Slider("X Noise Offset", cubes.xNoiseOffset, 0, 10);
        cubes.yNoiseOffset = EditorGUILayout.Slider("Y Noise Offset", cubes.yNoiseOffset, 0, 10);
        cubes.zNoiseOffset = EditorGUILayout.Slider("Z Noise Offset", cubes.zNoiseOffset, 0, 10);
        cubes.scale = EditorGUILayout.Slider("Scale", cubes.scale, .01f, .15f);
        cubes.isoLevel = EditorGUILayout.Slider("IsoLevel", cubes.isoLevel, 0f, 1f);
        cubes.octaves = EditorGUILayout.IntSlider("Octaves", cubes.octaves, 1, 4);
        cubes.lacunarity = EditorGUILayout.Slider("Lacunarity", cubes.lacunarity, 1f, 6f);
        cubes.persistance = EditorGUILayout.Slider("Persistance", cubes.persistance, .01f, 1f);

        if (GUI.changed) {
            cubes.Reset();
        }
    }
}
