﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Triangulation.CubePoint;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Cubes : MonoBehaviour {

    private Mesh mesh;

    private bool drawGizmos;
    private int width;
    private int length;
    private int height;
    private float xNoiseOffset;
    private float yNoiseOffset;
    private float zNoiseOffset;
    private float scale;
    private float isoLevel;

    public bool DrawGizmos { get => drawGizmos; set => drawGizmos = value; }
    public int Width { get => width; }
    public int Length { get => length; }
    public int Height { get => height; set => height = value; }
    public float XNoiseOffset { get => xNoiseOffset; set => xNoiseOffset = value; }
    public float YNoiseOffset { get => yNoiseOffset; set => yNoiseOffset = value; }
    public float ZNoiseOffset { get => zNoiseOffset; set => zNoiseOffset = value; }
    public float Scale { get => scale; set => scale = value; }
    public float IsoLevel { get => isoLevel; set => isoLevel = value; }

    public Cubes(bool drawGizmos, int width, int length, int height, float xNoiseOffset, float yNoiseOffset, float zNoiseOffset, float scale, float isoLevel) {
        Reset(drawGizmos, width, length, height, xNoiseOffset, yNoiseOffset, zNoiseOffset, scale, isoLevel);
    }

    void Start() {
        Generate();
    }

    void Update() {
        Generate();
    }

    public void Reset(bool drawGizmos, int width, int length, int height, float xNoiseOffset, float yNoiseOffset, float zNoiseOffset, float scale, float isoLevel) {
        this.drawGizmos = drawGizmos;
        this.width = width;
        this.length = length;
        this.Height = height;
        this.xNoiseOffset = xNoiseOffset;
        this.yNoiseOffset = yNoiseOffset;
        this.zNoiseOffset = zNoiseOffset;
        this.scale = scale;
        this.isoLevel = isoLevel;

        Generate();
    }

    public void Generate() {
        if (mesh == null) {
            mesh = new Mesh();
        } else {
            mesh.Clear();
        }
        GetComponent<MeshFilter>().mesh = mesh;

        CreateMesh();
    }

    void CreateMesh() {
        int trianglesPerCube = 4 * 3;
        var black = new Color32(0, 0, 0, 255);
        var white = new Color32(255, 255, 255, 255);

        Vector3[] vertices = new Vector3[width * length * height * 12];
        // Color32[] colors = new Color32[width * length * height * 8];
        int[] triangles = new int[width * length * height * trianglesPerCube];

        int cube = 0;
        int currentTriangle = 0;
        for (int y = 0; y < height; y++) {
            for (int z = 0; z < length; z++) {
                for (int x = 0; x < width; x++, cube++) {
                    Vector3 origin = new Vector3(-width / 2f + x, y, -length / 2f + z);
                    int vertexOffset = cube * 12;

                    Vector3[] cubeVertices = CreateCubeVertices(origin);
                    Triangulation.CubePoint[] cubePoints = new Triangulation.CubePoint[8];
                    for (int i = 0; i < 8; i++) {
                        Vector3 vertex = cubeVertices[i];
                        float perlinValue = Mathf.PerlinNoise(xNoiseOffset + (vertex.x) * scale, zNoiseOffset + (vertex.z) * scale);
                        cubePoints[i] = new Triangulation.CubePoint(vertex, perlinValue * (1 - vertex.y / (float)height));
                        // colors[vertexOffset + i] = Color32.Lerp(black, white, perlinValue * (1 - y / (float)height));
                    }

                    int cubeIndex = Triangulation.CalculateCubeIndex(isoLevel, cubePoints);

                    /* Cube is entirely in/out of the surface */
                    if (Triangulation.IsTrivial(cubeIndex)) {
                        continue;
                    }

                    Vector3[] interpolatedVertices = Triangulation.GetVertices(isoLevel, cubeIndex, cubePoints);
                    for (int v = 0; v < interpolatedVertices.Length; v++) {
                        vertices[vertexOffset + v] = interpolatedVertices[v];
                    }

                    int[] interpolatedTriangles = Triangulation.GetTriangles(cubeIndex);
                    for (int t = 0; t < interpolatedTriangles.Length; t++) {
                        triangles[currentTriangle + t] = interpolatedTriangles[t] + vertexOffset;
                    }
                    currentTriangle += interpolatedTriangles.Length;
                }
            }
        }

        mesh.vertices = vertices;
        // mesh.colors32 = colors;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }


    Vector3[] CreateCubeVertices(Vector3 origin) {
        Vector3[] vertices = new Vector3[]{
            new Vector3(origin.x, origin.y, origin.z),
            new Vector3(origin.x, origin.y, origin.z+1),
            new Vector3(origin.x+1, origin.y, origin.z+1),
            new Vector3(origin.x+1, origin.y, origin.z),
            new Vector3(origin.x, origin.y+1, origin.z),
            new Vector3(origin.x, origin.y+1, origin.z+1),
            new Vector3(origin.x+1, origin.y+1, origin.z+1),
            new Vector3(origin.x+1, origin.y+1, origin.z)
        };
        return vertices;
    }

    int[] CreateCubeTriangles(int offset) {
        return new int[]{
            offset+0,offset+2,offset+1,
            offset+0,offset+3,offset+2,
            offset+0,offset+1,offset+4,
            offset+1,offset+5,offset+4,
            offset+1,offset+2,offset+5,
            offset+2,offset+6,offset+5,
            offset+2,offset+3,offset+6,
            offset+3,offset+7,offset+6,
            offset+3,offset+0,offset+7,
            offset+0,offset+4,offset+7,
            offset+4,offset+5,offset+6,
            offset+4,offset+6,offset+7
        };
    }

    void OnDrawGizmos() {
        if (mesh == null || !drawGizmos) {
            return;
        }

        for (int j = 0; j < mesh.vertices.Length; j++) {
            Gizmos.DrawSphere(mesh.vertices[j], .05f);
        }
    }
}
