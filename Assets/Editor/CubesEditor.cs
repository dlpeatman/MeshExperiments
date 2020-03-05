﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Cubes))]
public class CubesEditor : Editor {
    public override void OnInspectorGUI() {

        Cubes cubes = (Cubes)target;
        int width = EditorGUILayout.IntSlider("Width", cubes.Width, 1, 20);
        int length = EditorGUILayout.IntSlider("Length", cubes.Length, 1, 20);
        int height = EditorGUILayout.IntSlider("Height", cubes.Height, 1, 12);
        float xNoiseOffset = EditorGUILayout.Slider("X Noise Offset", cubes.XNoiseOffset, 0, 10);
        float yNoiseOffset = EditorGUILayout.Slider("Y Noise Offset", cubes.YNoiseOffset, 0, 10);
        float zNoiseOffset = EditorGUILayout.Slider("Z Noise Offset", cubes.ZNoiseOffset, 0, 10);
        float scale = EditorGUILayout.Slider("Scale", cubes.Scale, .01f, .15f);
        float isoLevel = EditorGUILayout.Slider("IsoLevel", cubes.IsoLevel, 0f, 1f);

        if (GUI.changed) {
            cubes.Reset(width, length, height, xNoiseOffset, yNoiseOffset, zNoiseOffset, scale, isoLevel);
        }
    }
}
