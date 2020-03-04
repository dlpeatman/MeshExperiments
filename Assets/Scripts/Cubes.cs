using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Cubes : MonoBehaviour {

    private Mesh mesh;


    private int width;
    private int length;
    private int height;
    private float xNoiseOffset;
    private float yNoiseOffset;
    private float zNoiseOffset;
    private float scale;
    private float isoLevel;

    public int Width { get => width; }
    public int Length { get => length; }
    public int Height { get => height; set => height = value; }
    public float XNoiseOffset { get => xNoiseOffset; set => xNoiseOffset = value; }
    public float YNoiseOffset { get => yNoiseOffset; set => yNoiseOffset = value; }
    public float ZNoiseOffset { get => zNoiseOffset; set => zNoiseOffset = value; }
    public float Scale { get => scale; set => scale = value; }
    public float IsoLevel { get => isoLevel; set => isoLevel = value; }

    public Cubes(int width, int length, int height, float xNoiseOffset, float yNoiseOffset, float zNoiseOffset, float scale, float isoLevel) {
        Reset(width, length, height, xNoiseOffset, yNoiseOffset, zNoiseOffset, scale, isoLevel);
    }

    void Start() {
        Generate();
    }

    void Update() {
        Generate();
    }

    public void Reset(int width, int length, int height, float xNoiseOffset, float yNoiseOffset, float zNoiseOffset, float scale, float isoLevel) {
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
        int trianglesPerCube = 12 * 3;
        var black = new Color32(0, 0, 0, 255);
        var white = new Color32(255, 255, 255, 255);

        Vector3[] vertices = new Vector3[width * length * height * 8];
        Color32[] colors = new Color32[width * length * height * 8];
        int[] triangles = new int[width * length * height * trianglesPerCube];

        int cubeIndex = 0;
        for (int y = 0; y < height; y++) {
            for (int z = 0; z < length; z++) {
                for (int x = 0; x < width; x++, cubeIndex++) {
                    Vector3 origin = new Vector3(-width / 2f + x, -height / 2f + y, -length / 2f + z);
                    int vertexOffset = cubeIndex * 8;
                    int triangleOffset = cubeIndex * trianglesPerCube;

                    Vector3[] cubeVertices = CreateCubeVertices(origin);
                    for (int i = 0; i < 8; i++) {
                        vertices[vertexOffset + i] = cubeVertices[i];
                        float perlinValue = Mathf.PerlinNoise(xNoiseOffset + (x + .1f) * scale, zNoiseOffset + (z + .1f) * scale);
                        colors[vertexOffset + i] = Color32.Lerp(black, white, perlinValue * (1 - y / (float)height));
                    }

                    int[] cubeTriangles = CreateCubeTriangles(vertexOffset);
                    for (int i = 0; i < trianglesPerCube; i++) {
                        triangles[triangleOffset + i] = cubeTriangles[i];
                    }
                }
            }
        }

        mesh.vertices = vertices;
        mesh.colors32 = colors;
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

    // void OnDrawGizmos() {
    //     if (mesh == null) {
    //         return;
    //     }

    //     for (int j = 0; j < mesh.vertices.Length; j++) {
    //         Gizmos.DrawSphere(mesh.vertices[j], .05f);
    //     }
    // }
}
